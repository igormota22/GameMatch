# Design

## Context

O projeto já tem contas/sessão (F1) e o perfil gamer com preferências
normalizadas (`perfis_preferencias`). O catálogo ainda não existe: não há
entidade `Jogo`, não há projeto de CLI e `seed/` contém apenas o README.

Motivação e escopo estão em `proposal.md`; o comportamento exigido está em
`specs/catalogo/spec.md`. Decisões globais (DT-05, DT-11, DT-14, DT-15, DT-16)
permanecem no [`docs/tdd.md`](../../../docs/tdd.md); este documento registra
apenas as decisões locais.

## Goals / Non-Goals

**Goals:**

- Snapshot reproduzível e versionado, gerado por um comando de coleta e
  consumido por um comando de sincronização idempotente.
- Modelo de catálogo simétrico ao do perfil (características normalizadas),
  pronto para o cálculo da F3.
- Contrato de consulta paginado, filtrável e testável, com estados vazios
  distinguíveis na interface.

**Non-Goals:**

- Tela de detalhe, favoritos e avaliações (F4); recomendações (F3).
- Remoção de jogos que desaparecem do snapshot (o TDD define apenas upsert;
  a limpeza seria uma operação manual de banco).
- Sincronização automática, endpoint administrativo ou agendamento.
- Multisseleção de filtros, ordenação configurável ou busca por texto completo.

## Decisions

### 1. CLI com dois subcomandos, lógica testável fora do `Program`

`GameMatch.Seed` é um console (`net10.0`) com `Program.cs` fino que resolve
configuração e despacha `coletar` e `sync`. A coleta recebe `HttpClient`; a
sincronização recebe o caminho do snapshot e o `GameMatchDbContext`.

- **Por quê:** mantém o CLI como composição e permite testar coleta (handler
  fake) e sincronização (diretório de fixtures + banco de teste) sem processo
  externo.
- **Alternativas descartadas:** script PowerShell/Node (menos testável e fora da
  stack); sync na inicialização da API (contraria DT-11).
- **Consequência:** o projeto precisa entrar em `GameMatch.sln` e as versões de
  `Microsoft.Extensions.Hosting`/`Http` em `Directory.Packages.props` (gestão
  central já adotada).

### 2. Esquema do snapshot

```jsonc
// seed/jogos.json — array com os objetos retornados por /api/games
[{ "id": 540, "title": "Overwatch", "thumbnail": "...", "short_description": "...",
   "game_url": "...", "genre": "Shooter", "platform": "PC (Windows)",
   "publisher": "...", "developer": "...", "release_date": "...",
   "freetogame_profile_url": "..." }]

// seed/tags.json — mapa tag -> ids externos
{ "tags": { "3d": [1, 2, 3], "action": [2, 4] } }

// seed/metadados.json
{ "coletadoEm": "2026-11-16T12:00:00Z", "totalJogos": 416, "totalTags": 30,
  "tagsIgnoradas": ["third-person", "zombies"],
  "origem": "https://www.freetogame.com/api", "versaoFormato": 1 }
```

- **Por quê:** preserva o payload da fonte (auditável) e mantém o mapa de tags
  independente, como previsto no `seed/README.md`.
- **Alternativa descartada:** transformar os dados na coleta — perderia a
  rastreabilidade da fonte e acoplaria coleta e modelo.
- **Validação no `sync`:** `id` e `title` obrigatórios; ids duplicados no array
  são erro de formato; `coletadoEm` obrigatório; tags órfãs são aviso, não erro.

### 3. Lista curada de tags candidatas

A API não expõe a lista de tags (`/api/tags` devolve HTML da SPA e as páginas
não trazem os links no server-side). A coleta itera uma lista configurada de 32
nomes conhecidos (`3d`, `action`, `anime`, `battle-royale`, `card`, `fantasy`,
`fighting`, `first-person`, `horror`, `low-spec`, `military`, `mmo`, `mmofps`,
`mmorpg`, `moba`, `open-world`, `pvp`, `racing`, `sandbox`, `sci-fi`, `shooter`,
`space`, `sports`, `strategy`, `survival`, `tank`, `third-person`, `top-down`,
`tower-defense`, `turn-based`, `voxel`, `zombies`) via `/api/filter?tag=<tag>`.

- **Por quê:** 30 das 32 retornam jogos hoje; `third-person` e `zombies`
  respondem 500. A lista em configuração (`Coleta:TagsCandidatas`) permite
  ajustar sem recompilar e o resultado fica registrado em `metadados.json`.
- **Alternativa descartada:** raspar o bundle JS da SPA — frágil e sem contrato.
- **Consequência:** tag nova só entra no snapshot após atualização da
  configuração; falha de uma tag não invalida a coleta.

### 4. Modelo de dados e mapeamento

- `Jogo`: `Id`, `IdExterno` (único), `Titulo`, `Descricao` (`short_description`),
  `Thumbnail`, `Desenvolvedor`, `UrlFonte` (`freetogame_profile_url`),
  `SincronizadoEm`.
- `JogoCaracteristica`: `(JogoId, Tipo, Valor)` com
  `TipoCaracteristica { Genero = 1, Plataforma = 2, Estilo = 3 }` — tags são
  `Estilo` (DT-08).
- Tabelas `jogos` e `jogos_caracteristicas`; índice único em `id_externo`,
  `(tipo, valor)` para filtros e FK em cascata.

- **Por quê:** espelha `perfis_preferencias` (DT-06/DT-15) e atende ao cálculo
  da F3 sem junções específicas.
- **Decisão:** `SincronizadoEm` recebe a data de coleta do `metadados.json`, não
  o instante da execução do `sync` — a coluna expressa a proveniência do dado.
- **Decisão:** `publisher` e `release_date` ficam apenas no snapshot, fora do
  modelo e do contrato.
- **Consequência:** cada jogo tem exatamente 1 gênero e 1 plataforma (a API
  devolve strings únicas) e N tags.

### 5. Upsert transacional

O `sync` valida todos os arquivos antes de abrir a transação; dentro dela,
carrega os jogos existentes por `IdExterno` em lote, insere os ausentes,
atualiza os presentes e substitui o conjunto de características de cada jogo.
Ao final, imprime `inseridos`, `atualizados`, `tags ignoradas` e `coletadoEm`.

- **Por quê:** atende INV-03 e a exigência de não alterar o banco em caso de
  snapshot inválido; substituir características evita linhas órfãs quando uma
  tag sai do snapshot.
- **Alternativa descartada:** `DELETE`+`INSERT` do catálogo inteiro — mudaria os
  ids internos e quebraria futuras FKs de favoritos/avaliações.
- **Consequência:** jogos removidos da fonte permanecem no banco; limpeza é
  manual e documentada.

### 6. Contrato de consulta

| Item | Decisão |
|---|---|
| Paginação | `pagina` (padrão 1, mínimo 1) e `tamanho` (padrão 24, 1–100); violação → 400 |
| Ordem | `Titulo` ascendente com `Id` como desempate (paginação estável) |
| Busca | `ILIKE` em `Titulo` com escape de `\`, `%` e `_`; termo literal |
| Filtros | `genero`, `plataforma`, `tag` aceitam um valor cada, comparação exata, combinados com E lógico; strings vazias ignoradas |
| `tag` | consulta `Tipo = Estilo` |
| Total | `CountAsync` sobre a mesma consulta filtrada, sem paginação |
| Detalhe | rota tipada `{id:int}`; identificador não numérico ou ausente → 404 |
| DTOs | `JogoResumoDto { id, titulo, thumbnail, generos[], plataformas[], desenvolvedor }`; `JogoDetalheDto` acrescenta `descricao`, `urlFonte` e `tags[]`; `FiltrosCatalogoDto { generos[], plataformas[], tags[] }`; `PaginaDto<T> { items[], pagina, tamanho, total }` |

- **Por quê:** segue o contrato do TDD §5.1 e mantém o resumo compatível com o
  `jogo` embutido no `RecomendacaoDto` da F3.
- **Alternativa descartada:** filtros multivalorados (`genero=RPG&genero=Ação`)
  — exigiria mudar o contrato do TDD; fica para depois do MVP.

### 7. Estado da interface

`features/catalogo` usa signals e formulário reativo: busca com `debounceTime`
de 300 ms e `distinctUntilChanged`; selects de gênero, plataforma e tag aplicam
imediatamente e voltam a página para 1. Cada consulta envia todos os filtros ao
servidor (sem filtragem no cliente).

Estados explícitos: carregando, erro, **catálogo vazio** (`total = 0` e nenhum
filtro/busca ativo) e **sem resultados** (`total = 0` com filtro/busca ativo,
com “Limpar filtros”). O filtro de tag é rotulado “Tag” e corresponde aos
estilos de gameplay (DT-08).

- **Por quê:** separa CA-08 de CA-10 e evita paginação inconsistente.
- **Alternativa descartada:** sincronizar filtros na URL — adiado; não é
  requisito e aumenta a superfície de teste.
- **Acessibilidade:** `<label>` em todos os controles, navegação por teclado,
  `aria-live` com a contagem de resultados e foco no título da lista ao trocar de
  página; imagens com `alt` e `loading="lazy"`.

### 8. Estratégia de testes

- **Unitários:** leitura/validação do snapshot com fixtures em
  `api/tests/GameMatch.UnitTests/Catalogo/Fixtures` (válido, arquivo ausente,
  JSON inválido, id duplicado, tag órfã); coletor com `HttpMessageHandler` fake
  (sucesso, tag 500, falha de rede, escrita atômica); validação de paginação.
- **Integração:** `sync` a partir de um diretório de fixtures e execuções
  repetidas; endpoints (401, 400, 404, 200, filtros combinados, paginação e
  detalhe). O banco de testes é compartilhado entre classes: cada teste usa
  `IdExterno` e termos únicos e afirma sobre subconjuntos, nunca sobre o total
  global.
- **Frontend:** `HttpTestingController` para o serviço; componente com cenários
  de carga, resultados, catálogo vazio, sem resultados, limpar filtros,
  paginação e erro; guarda e link do cabeçalho.

- **Por quê:** cobre TC-02 nos níveis previstos em DT-16, sem depender de rede
  nem de `seed/` real nos testes.

## Risks / Trade-offs

- **Fragilidade da API da FreeToGame** (duas tags já retornam 500) → coleta
  tolerante, lista de tags em configuração, snapshot versionado e reexecução
  manual; nenhuma dependência de rede em tempo de execução.
- **Snapshot grande no repositório** (~200 KB por arquivo) → apenas JSON, sem
  mídia; versionado de propósito para a demonstração offline.
- **Busca sensível a acentos** (colação do Postgres) → títulos são em inglês;
  limitação documentada e busca por filtros usa valores exatos das listas.
- **Jogos removidos da fonte permanecem no banco** → comportamento do upsert do
  TDD; documentado em `docs/ambiente.md` como limpeza manual.
- **Testes de integração compartilham o catálogo** → ids/termos únicos por teste
  e asserções por subconjunto.
- **`sync` exige string de conexão como a API** → mesmo mecanismo
  (`ConnectionStrings__GameMatch`), documentado no `docs/ambiente.md`.

## Migration Plan

1. Gerar e aplicar a migration `CatalogoJogos` (`jogos` e
   `jogos_caracteristicas`) no PostgreSQL local.
2. Executar `dotnet run --project api/src/GameMatch.Seed -- coletar` e depois
   `-- sync`; versionar o snapshot gerado.
3. Validar `/catalogo`, `/catalogo/filtros` e `/catalogo/{id}` e a tela
   `/catalogo` no navegador.
4. **Rollback:** `dotnet ef database update PerfilGamer` remove as duas tabelas
   novas; como não há FKs de outras fases, não há perda de dados de usuários.
   Reverter o snapshot é opcional (a aplicação volta ao estado vazio).

## Open Questions

- Frequência de atualização do snapshot: permanece manual e sob demanda; não
  altera contrato, abordagem ou tarefas.

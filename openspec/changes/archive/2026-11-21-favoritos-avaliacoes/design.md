# Design

## Context

F1–F3 já entregaram perfil, catálogo e recomendações. Favoritos e avaliações
não existem em nenhum nível (domínio, persistência, API ou web), e a exclusão
de itens já marcados nas recomendações ficou registrada como pendente na
change `recomendacoes-descoberta` (DT-12/INV-06). O contrato de endpoints da
§5.1 do `docs/tdd.md`, as invariantes INV-04..07 e a estrutura em camadas
Domain/Application/Infrastructure/Api já estão definidos e devem ser seguidos.
Ver `proposal.md` para a motivação completa.

## Goals / Non-Goals

**Goals:**

- Entregar F4 conforme contratos da §5.1 do TDD, com TC-07 verde.
- Aplicar a exclusão DT-12 nas recomendações sem alterar o formato das
  respostas existentes.
- Estado de favorito visível nos cards do catálogo e das recomendações sem
  duplicar lógica de UI.

**Non-Goals:**

- Histórico de jogos explorados/avaliados (PRD Q7), perfil público de
  avaliações (§5.2) e cache de recomendações (P-05) — pós-MVP.
- Página de detalhe do jogo: a favoritação acontece nos cards existentes e a
  avaliação apenas na tela `/favoritos` (escopo de UI escolhido na entrevista
  desta change).
- Novos campos nos DTOs de catálogo/recomendação (ver D1).

## Decisions

### D1 — Estado de favorito nos cards via `GET /favoritos/ids`

Para o botão de favoritar refletir estado nos cards, o cliente precisa saber
quais jogos estão favoritados. Alternativas avaliadas:

- **(adotada)** Novo endpoint mínimo `GET /favoritos/ids` → `{ "ids": [] }`,
  consulta apenas da PK composta por usuário, devolvida em uma chamada.
  Contrato pequeno, não toca nos DTOs existentes e é barata mesmo com muitos
  favoritos.
- Incluir `favorito: boolean` no `JogoResumoDto`/`RecomendacaoDto`: muda o
  contrato de dois endpoints já entregues e mistura dados do usuário em
  DTOs de catálogo — rejeitado.
- Reusar `GET /favoritos?tamanho=grande` no cliente: frágil diante do
  "sem limite de quantidade" (RB-14) — rejeitado.

**Consequência:** `GET /favoritos/ids` é um acréscimo à §5.1 do TDD; o mesmo
PR atualiza `docs/tdd.md` (regra de operação `apply` do `openspec/config.yaml`).

### D2 — Exclusão DT-12 na camada de leitura, filtro no caso de uso

`IRecomendacaoRepositorio` passa a devolver, junto do catálogo, o conjunto de
`JogoId` favoritados/avaliados do usuário (uma leitura em lote da PK
composta, `AsNoTracking`). O `ServicoRecomendacoes` filtra o catálogo em
memória antes de calcular/ordenar/top-12. Alternativa: filtro em SQL dentro
da consulta do catálogo — rejeitada porque o cálculo já é em memória (DT-10)
e uma cláusula `NOT IN` acoplada ao repositório de catálogo esconderia a
regra de negócio INV-06 do caso de uso, onde o TDD manda aplicá-la.

### D3 — Serviço Angular compartilhado de marcações

`FavoritosService` (novo, em `core/servicos/`) mantém um `signal` com o
conjunto de ids favoritados, carregado uma vez por sessão a partir de
`GET /favoritos/ids` e atualizado otimistamente nas operações de favoritar/
remover. `features/catalogo` e `features/recomendacoes` injetam o mesmo
serviço (padrão dos serviços já existentes); `features/favoritos` consome a
lista paginada diretamente de `GET /favoritos`. Avaliação (estrelas) fica
apenas em `/favoritos`, evitando estado de avaliação duplicado nos cards.

### D4 — Idempotência e concorrência na camada de persistência

- `POST /favoritos/{jogoId}`: inserção com `ON CONFLICT DO NOTHING`; se nenhuma
  linha foi criada, responde 200 (INV-05). Dois POSTs paralelos nunca criam
  duplicata (PK composta).
- `PUT /avaliacoes/{jogoId}`: *upsert* na PK composta; dois PUTs paralelos
  convergem para um único registro com a última nota (cenário de concorrência
  da §7.2).
- `UsuarioId` sempre do token (`Sub`), como já fazem os controllers
  existentes (INV-07); nunca do corpo/URL.

### D5 — Ordem da listagem de favoritos

`GET /favoritos` ordena por `CriadoEm` descendente (mais recentes primeiro),
com paginação reusando `PaginaDto<JogoResumoDto>` e os parâmetros
`pagina`/`tamanho` do catálogo. Alternativa: ordenação alfabética —
descartada por não destacar as marcações recentes; a remoção de um item
durante a paginação pode deslocar itens entre páginas, aceito no MVP
(sem ordenação estável garantida).

## Risks / Trade-offs

- [Acréscimo de endpoint fora da §5.1 original] → atualizar `docs/tdd.md` no
  mesmo PR (D1) e referenciar DT-12/INV-06.
- [Estado otimista de favorito divergir da API em falha de rede] → reverter o
  signal e exibir mensagem de erro via `mensagemDeErro` (padrão existente).
- [Exclusão reduzir o top-12 para usuários muito ativos] → risco conhecido e
  aceito em DT-12; verificação no TC-07 com cenário de catálogo parcialmente
  marcado.
- [Migration nova em banco já com dados] → tabela vazia, sem dados a
  transformar; rollback = reverter para a migration anterior + redeploy
  (procedimento padrão do TDD §4.4).

## Migration Plan

1. Migration EF `FavoritosAvaliacoes` (entidades + índices + CHECK de nota),
   aplicada sequencialmente após as existentes.
2. Deploy único da API + web (projeto acadêmico, ambiente local/demonstração;
   sem etapa de feature flag).
3. Rollback: reverter o código e rodar a migration anterior (§4.4 do TDD).

## Open Questions

- Nenhuma. As decisões de UI foram confirmadas na entrevista desta change
  (cards + tela `/favoritos`).

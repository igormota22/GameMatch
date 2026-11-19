# Design

## Context

F1/F2/F2.5 já entregaram perfil (`ServicoPerfil` + `IPerfilRepositorio`),
catálogo (`ServicoCatalogo` + `ICatalogoRepositorio`) e o vocabulário comum
entre os dois (DT-18). Não existe nada de recomendação, favorito ou avaliação no
código (`api/src` e `web/src` sem menções). O padrão do projeto é: contrato na
Application, implementação EF na Infrastructure com `AsNoTracking`, caso de uso
`sealed` com mapeamento manual, controlador `[Authorize]` devolvendo
`Problem(errors)`, e DI com `AddScoped` no `Program.cs`. Consulta de configuração
é feita lendo `IConfiguration` diretamente (padrão de `Jwt:*`), sem
`IOptions<T>`.

Ver `proposal.md` (Why/What Changes) e `specs/recomendacoes/spec.md` para o
comportamento exigido; este documento cobre apenas decisões locais.

## Goals / Non-Goals

**Goals:**

- Entregar F3: `GET /recomendacoes?experiencia=`, `GET /recomendacoes/opcoes`
  e rota `/recomendacoes` com explicação, CTA e seletor de experiência.
- Manter o cálculo como regra pura testável sem banco nem HTTP (DT-17).
- Configurar limites e opções sem recompilar (DT-09, DT-14).

**Non-Goals:**

- Excluir jogos já favoritados/avaliados (DT-12, INV-06) — sem dados até F4;
  nenhuma fonte de exclusão é criada nesta change.
- Persistir recomendações ou perfis de interesse (DT-10: cálculo sob demanda).
- Alterar requisitos de `catalogo` ou `perfil-gamer`.
- Alterar a fórmula ou os pesos (DT-07 é global, em `docs/tdd.md`).

## Decisions

**D1 — Calculadora em `GameMatch.Domain/Recomendacoes/`.**
`CalculadoraCompatibilidade.Calcular(Preferencias, Caracteristicas, PesoExperiencia?)`
devolve `{ Pontuacao, Fatores[] }`, sem EF/HTTP. Alternativa descartada: cálculo
em `Application` ou em SQL — a primeira quebra a fronteira de camadas prevista em
`docs/arquitetura.md`, a segunda transfere regra de negócio para o banco e
dificulta o unit test (TC-03).

**D2 — Caso de uso carrega tudo em duas leituras e calcula em memória.**
`IRecomendacaoRepositorio` devolve preferências do usuário, catálogo completo
com características e (futuro) pares de exclusão; `ServicoRecomendacoes`
orquestra: valida sessão/perfil → resolve opção de experiência → chama a
calculadora → ordena → aplica o limite. Alternativa descartada: paginação no
banco — o topo-12 depende de ordenar por pontuação calculada (DT-14), que só
existe em memória.

**D3 — Configuração sem `IOptions`.**
`Recomendacoes:Top` (int, padrão 12) e `Descoberta:Opcoes` (lista de
`{ Chave, Rótulo, Generos[], Tags[] }`) lidos de `IConfiguration` em classes de
opções simples (`OpcoesRecomendacoes`, `OpcoesDescoberta`) registradas na DI.
Mantém o padrão de `Jwt:*` e evita introduzir `Configure<Options>` só para esta
feature. Alternativa: `IOptions<T>` — mais idiomático no ecossistema ASP.NET,
mas cria um segundo padrão no repositório sem ganho; se surgir uma terceira
seção de opções, vale revisar.

**D4 — Erros como resultado do caso de uso, não exceções.**
`ResultadoRecomendacoes { Sucesso, Erros, Itens }` mapeado no controlador:
`409` com título `perfil-incompleto` quando faltam gêneros/plataformas, `404`
para `experiencia` desconhecida, `422` nunca ocorre aqui. Segue o padrão de
`ResultadoSalvarPerfil` e mantém `traceId` do `AddProblemDetails` global.

**D5 — `experiencia` é validada contra a configuração, não contra o banco.**
A chave só existe se estiver em `Descoberta:Opcoes`; a validação ocorre antes
de qualquer leitura de catálogo, evitando trabalho descartado em requisição
inválida.

**D6 — Contrato de explicação estável.**
Fatores sempre na ordem `generos`, `plataformas`, `estilos` (+ `experiencia`
quando aplicável), com `chave`, `rotulo`, `contribuicao` e `correspondencias`.
`contribuicao = round(peso × 100 × fator)` após renormalização; a soma pode
diferir da pontuação em ±1 — documentado em teste (TC-04), conforme TDD §5.1.

**D7 — Ordenação determinística.**
Pontuação decrescente e, em empate, `Id` ascendente, para respostas estáveis
entre chamadas e entre ambientes de teste.

**D8 — Frontend: um serviço, uma rota lazy, três estados.**
`RecomendacoesService` (`/api/recomendacoes`, `/api/recomendacoes/opcoes`),
`features/recomendacoes/` com rota protegida por `autenticadoGuard` e item no
`cabecalho.html`. A tela distingue carregando, `perfil-incompleto` (CTA para
`/perfil`) e erro de API (mensagem em português, navegação preservada —
RNF-05). O seletor busca `/recomendacoes/opcoes` e reenvia a chave como query
string.

**D9 — Testes nos padrões existentes.**
Unit puro para a calculadora (renormalização, arredondamento, 0–100, ordenação)
com fakes privados no próprio arquivo; integração reutilizando
`FabricaApiComBanco` + seed de catálogo no estilo de `PerfilTests` e
autenticação por `POST /contas`. Web: spec do componente com
`provideHttpClientTesting` e `expectOne`.

**D10 — Tradução das descrições no snapshot, não no banco.**
A `descricao` exibida nos cards vem de `seed/jogos.json` (`short_description`),
que é a fonte de verdade: o `sync` reescreve `jogo.descricao` a cada
sincronização, então traduzir direto no banco seria revertido. As 416
descrições são traduzidas para PT-BR no próprio snapshot (preservando ids,
ordem e demais campos, que seguem o formato original da FreeToGame) e o banco
é re-sincronizado. Alternativa descartada: traduzir na camada de apresentação
— duplicaria regra de negócio na web e não resolveria `/catalogo`.

## Risks / Trade-offs

- [Pesos produzem poucos jogos com pontuação relevante (R-1 do PRD)] →
  pesos centralizados no Domain; a resposta retorna o que houver e o teste
  valida 0–100 sem exigir corte mínimo de pontuação.
- [Cálculo O(N) a cada chamada (~416 jogos)] → aceito por DT-10/DT-14; se a
  medição de Q5 apontar problema, introduzir cache no caso de uso sem mudar a
  calculadora.
- [DT-12 sem implementação até F4] → registrado como Non-Goal; INV-06 continua
  pendente no TDD e será coberto pela change de favoritos/avaliações.
- [Duas leituras de catálogo por requisição] → `AsNoTracking` + uma única
  projeção; se o volume crescer, materializar a consulta em lote antes de
  qualquer cache.

## Migration Plan

Sem migração de dados: nada é persistido. Deploy apenas adiciona endpoint,
rota e configurações; rollback = reverter o commit (nenhum dado precisa ser
removido). `appsettings.json` ganha seções com valores padrão já funcionais,
de forma que uma API desatualizada responde normalmente à web atual e vice-versa.

## Open Questions

- Tempo de resposta esperado (RNF-06 / Q-02): medir no ambiente local durante a
  implementação e registrar em `docs/tdd.md` se houver orçamento definido.
- Rótulos e mapeamentos finais das cinco opções de DT-09: revisão de produto
  após a entrega, alterando só `Descoberta:Opcoes`.

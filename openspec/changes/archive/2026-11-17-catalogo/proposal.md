# Proposal

## Why

As recomendações da F3 dependem de um catálogo real de jogos gratuitos, e hoje o
GameMatch não possui nenhum dado de jogos: o `seed/` contém apenas o README e não
existe entidade, sincronização nem interface de catálogo. Esta change executa a
fase **F2 — Catálogo** do roadmap do [`docs/tdd.md`](../../../docs/tdd.md),
atendendo aos requisitos **RF-07/RF-08/RF-09** e aos critérios **CA-08/CA-09/CA-10**
do [`docs/prd.md`](../../../docs/prd.md), sob as decisões **DT-11** (sincronização
por CLI manual) e **DT-15** (catálogo com características normalizadas), e cobre o
critério técnico **TC-02**.

O snapshot local em `seed/` é pré-requisito declarado da F2 (D-1/R-2) e não
existe. Esta change resolve a pendência gerando o snapshot a partir da API
pública da FreeToGame por meio de um comando de coleta versionado, mantendo a
aplicação totalmente offline e somente leitura sobre `seed/`.

## What Changes

- Incluir no projeto de CLI `GameMatch.Seed` um subcomando `coletar` que gera
  `seed/jogos.json`, `seed/tags.json` e `seed/metadados.json` a partir da API da
  FreeToGame, com tolerância a tags indisponíveis e escrita atômica (decisão
  aprovada nesta change; complementa DT-11).
- Implementar o subcomando `sync` que valida o snapshot, faz upsert idempotente
  por `IdExterno` e substitui as características do jogo na mesma transação,
  sem nunca escrever em `seed/`.
- Criar a entidade `Jogo` e as características normalizadas
  `(JogoId, Tipo, Valor)` para gênero, plataforma e tag (DT-15), com migration
  própria.
- Expor os endpoints autenticados `GET /catalogo` (paginação, busca e filtros),
  `GET /catalogo/filtros` e `GET /catalogo/{id}`, com validação e erros em
  ProblemDetails (DT-05).
- Criar a rota lazy autenticada `/catalogo` no frontend com busca por nome,
  filtros de gênero, plataforma e tag, cards com dados do jogo, paginação,
  estado de catálogo não sincronizado e estado de busca sem resultados com ação
  de limpar filtros (CA-08/CA-10), além do link de acesso no cabeçalho.
- Cobrir a mudança com testes unitários, de integração (contrato, filtros,
  paginação e idempotência do `sync`) e de componente, conforme DT-16.
- Atualizar `seed/README.md`, `docs/ambiente.md`, `docs/tdd.md` (§5.4 e §8),
  `docs/arquitetura.md` e `README.md` com o snapshot, os comandos e os novos
  endpoints.

**Fora desta change:** cálculo e endpoints de recomendações e Modo Descoberta
(F3); tela de detalhe do jogo, favoritos e avaliações (F4) — o endpoint de
detalhe é entregue aqui apenas como contrato, sem rota de interface. Filtros
multi-valor, ordenação alternativa, atualização automática do catálogo na
inicialização e CI permanecem fora do escopo (DT-11, DT-16). `publisher` e
`release_date`, presentes no snapshot, não entram no modelo nem no contrato.

## Capabilities

### New Capabilities

- `catalogo`: snapshot e sincronização do catálogo da FreeToGame, consulta
  paginada com busca e filtros, listas de filtros disponíveis, detalhe do jogo e
  apresentação do catálogo na interface autenticada.

### Modified Capabilities

Nenhuma. A capability `perfil-gamer` existente não tem requisitos alterados por
esta change.

## Impact

- **Backend:** novas entidades e enum em `GameMatch.Domain`, DTOs, caso de uso e
  repositório em `GameMatch.Application`, configuração EF e migration em
  `GameMatch.Infrastructure`, `CatalogoController` em `GameMatch.Api`.
- **Banco:** duas tabelas novas (`jogos` e `jogos_caracteristicas`) e uma
  migration; nenhuma alteração destrutiva nas tabelas existentes.
- **CLI:** novo projeto `GameMatch.Seed` na solução, com os comandos `coletar` e
  `sync`; passa a exigir a string de conexão do mesmo modo que a API.
- **Dados:** `seed/` deixa de ser vazio e passa a versionar o snapshot coletado;
  a aplicação continua sem escrita nesse diretório.
- **Frontend:** nova feature `features/catalogo`, serviço em `core/servicos`,
  rota autenticada `/catalogo` e link no cabeçalho.
- **Contrato:** `/catalogo`, `/catalogo/filtros` e `/catalogo/{id}`, sem
  versionamento (DT-05); nenhuma quebra nos endpoints existentes.
- **Testes:** `GameMatch.UnitTests`, `GameMatch.IntegrationTests` e runner do
  Angular CLI (DT-16).

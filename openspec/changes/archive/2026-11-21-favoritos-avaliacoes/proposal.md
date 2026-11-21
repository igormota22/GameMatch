# Proposal

## Why

O projeto concluiu F1, F2, F2.5 e F3 (perfil, catálogo e recomendações com
Modo Descoberta), mas o usuário ainda não consegue marcar jogos de interesse
nem registrar opinião sobre eles — comportamento previsto no PRD (RF-16..18,
jornadas J-6/J-7) e previsto no roadmap do `docs/tdd.md` §8 como **F4**,
próxima fase do MVP. A F4 também é a condição para entregar a exclusão de
jogos já favoritados/avaliados nas recomendações (DT-12, INV-06), que ficou
explicitamente pendente na change `recomendacoes-descoberta`.

## What Changes

- **Favoritos:** `POST /favoritos/{jogoId}` (201 na criação, **200
  idempotente** se já existir — INV-05), `DELETE /favoritos/{jogoId}` (204),
  `GET /favoritos?tamanho=&pagina=` (paginação opcional, `PaginaDto<JogoResumoDto>`),
  todos sob sessão autenticada com `UsuarioId` derivado do token (INV-07).
- **Avaliações:** `GET /avaliacoes/{jogoId}`, `PUT /avaliacoes/{jogoId}` com
  `{ nota: 1..5 }` (cria ou substitui — RB-15; 422 fora da faixa, INV-04) e
  `DELETE /avaliacoes/{jogoId}` (204).
- **Entidades e persistência:** `Favorito` (PK composta
  `(UsuarioId, JogoId)`, `CriadoEm`) e `Avaliacao` (PK composta, `Nota` com
  CHECK 1–5, `AtualizadaEm`) em `GameMatch.Domain`/`Infrastructure`, com
  migration EF e índices `Favoritos (UsuarioId)` e `Avaliacoes (UsuarioId)`
  (§4.1/4.3 do TDD).
- **Exclusão nas recomendações (DT-12, INV-06):** `GET /recomendacoes` deixa
  de sugerir jogos que o usuário já favoritou ou avaliou (continuam no
  catálogo); comportamento novo na capability `recomendacoes`.
- **Interface web:** botão de favoritar nos cards do catálogo e das
  recomendações; nova rota autenticada `/favoritos` listando os favoritos com
  remoção e controle de estrelas 1–5 para avaliar/alterar/remover a avaliação
  (J-6, J-7, CA-16/17); entrada de navegação no cabeçalho autenticado.
- **Fora do escopo (registrado):** histórico de jogos avaliados/explorados
  (PRD Q7), perfil público com avaliações visíveis (§5.2) e exclusão de conta
  (Q6) permanecem pós-MVP; nenhuma mudança nos requisitos de `catalogo` e
  `perfil-gamer`.

## Capabilities

### New Capabilities

- `favoritos-avaliacoes`: favoritar/remover sem duplicados e sem limite,
  listar favoritos, avaliar de 1 a 5 estrelas com uma avaliação por jogo
  editável e removível, e proteção dos dados do próprio usuário.

### Modified Capabilities

- `recomendacoes`: passa a excluir jogos já favoritados ou avaliados pelo
  usuário antes de ordenar/limitar o top 12 (DT-12, INV-06, TC-07).

## Impact

- **Domínio:** novas entidades `Favorito` e `Avaliacao` em
  `GameMatch.Domain/Entidades/` (tipos de valor e regras puras, DT-17).
- **Infraestrutura:** `Configuracoes/` das duas entidades, migration EF nova
  (sem editar migration aplicada), índices por usuário; repositório de
  favoritos/avaliações no padrão dos existentes.
- **Aplicação:** novos `Application/Favoritos/` e `Application/Avaliacoes/`
  (repositório, serviço, DTOs); `IRecomendacaoRepositorio`/`ServicoRecomendacoes`
  ganham a exclusão dos pares já marcados.
- **API:** novos `FavoritoController` e `AvaliacaoController`
  (`[Authorize]`, ProblemDetails com `traceId`, contratos da §5.1 do TDD);
  `RecomendacaoController` sem mudança de rota.
- **Web:** novos `core/servicos/favoritos.service.ts` e
  `avaliacoes.service.ts`, componente `features/favoritos/`, ação de favoritar
  em `features/catalogo` e `features/recomendacoes`, rota em `app.routes.ts` e
  item no `cabecalho.html`.
- **Testes:** unit (`GameMatch.UnitTests`) de validação de nota e regras de
  favorito/avaliação, integração (`GameMatch.IntegrationTests`) de
  idempotência, 404/422/201/200/204, INV-05..07 e concorrência (TC-07), specs
  de componente Angular (CA-16/17).
- **Documentação:** PRD/TDD só se a implementação alterar decisão registrada;
  F4 concluída atualiza o roadmap do `docs/tdd.md` §8 e o `README.md`.

# Proposal

## Why

O projeto completou F1 (perfil), F2 (catálogo) e F2.5 (vocabulário do catálogo),
mas ainda não entrega a razão de existir do produto: transformar preferências
e catálogo em sugestões. F3 desbloqueada por DT-07 (fórmula confirmada) e por
DT-18 (perfil já fala o mesmo vocabulário do catálogo), é a próxima fase do
roadmap do `docs/tdd.md` §8.

## What Changes

- Motor de compatibilidade **puro** no `GameMatch.Domain`
  (`CalculadoraCompatibilidade`): pontuação 0–100 com pesos de DT-07
  (gêneros 40%, plataformas 20%, estilos 25%, experiência 15% no Modo
  Descoberta), fator sem dados do usuário ignorado e pesos renormalizados,
  fatores devolvidos para explicação (RF-10, RF-11).
- `GET /recomendacoes?experiencia=` (DT-13): top 12 ordenado por pontuação
  (DT-14), cálculo sob demanda em memória (DT-10), 401, **409
  `perfil-incompleto`** quando faltarem gêneros/plataformas (CA-13) e 404
  para `experiencia` desconhecida.
- `GET /recomendacoes/opcoes`: lista fixa do Modo Descoberta vinda de
  `Descoberta:Opcoes` no `appsettings` (DT-09), com chave e rótulo.
- Rota `/recomendacoes` (guard autenticado) com listagem explicada
  (pontuação e contribuição por fator), seletor do Modo Descoberta e CTA de
  perfil incompleto quando a API responder 409 (RF-13, RF-14, RF-15).
- Entrada de navegação no cabeçalho autenticado e nova seção em
  `appsettings.json`.
- **Fora do escopo (registrado):** a exclusão de jogos já favoritados ou
  avaliados (DT-12, INV-06) será entregue junto com F4, porque favoritos e
  avaliações ainda não existem; nesta change nenhuma exclusão é aplicada.
- Nenhuma mudança de comportamento nos requisitos já especificados de
  `catalogo` e `perfil-gamer`.

## Capabilities

### New Capabilities

- `recomendacoes`: pontuação de compatibilidade entre o perfil e o catálogo,
  explicação dos fatores, indisponibilidade com perfil incompleto e Modo
  Descoberta com opções predefinidas.

### Modified Capabilities

<!-- Nenhuma. -->

## Impact

- **Domínio:** novos `GameMatch.Domain/Recomendacoes/` (calculadora e tipos de
  valor puros, sem EF/HTTP — DT-17).
- **Aplicação:** novos `Application/Recomendacoes/`
  (`IRecomendacaoRepositorio`, `ServicoRecomendacoes`, DTOs de resposta) e
  leitura de `Descoberta:Opcoes` via `IConfiguration` (padrão já usado em
  `Jwt:*`).
- **Infraestrutura:** novo `Persistencia/Repositorios/RecomendacaoRepositorio.cs`
  (preferências + catálogo + características em duas leituras em lote,
  `AsNoTracking`); sem migração de dados.
- **API:** novo `Controllers/RecomendacaoController.cs`
  (`[Authorize]`, ProblemDetails com `traceId`); `Program.cs` recebe dois
  registros de serviço.
- **Web:** nova `features/recomendacoes/`, serviço em `core/servicos/`, rota
  protegida em `app.routes.ts` e item no `cabecalho.html`.
- **Configuração:** `Descoberta:Opcoes` e `Recomendacoes:Top` em
  `appsettings.json` (valores de DT-09 e DT-14).
- **Testes:** unit de fórmula/ordenação/renormalização (TC-03/04), integração de
  409/404/top-12 (TC-05/06) e specs do componente (CA-11..CA-15).
- **Documentação:** PRD/TDD só se a implementação alterar decisão registrada;
  F3 concluída atualiza o roadmap do `docs/tdd.md` §8 e o `README.md`.

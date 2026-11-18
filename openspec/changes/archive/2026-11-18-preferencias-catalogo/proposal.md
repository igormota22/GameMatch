# Proposal

## Why

O perfil gamer ainda aceita gêneros, plataformas e estilos como texto livre,
enquanto o catálogo já mantém o vocabulário real dos jogos sincronizados. Sem
essa sincronização, a próxima fase (recomendação) compara valores digitados
manualmente com características do catálogo que podem nunca coincidir. Esta
change alinha o formulário de `/perfil` às opções do catálogo e faz a API
garantir esse vocabulário (PRD RF-05, RB-16, CA-21/22; TDD DT-18).

## What Changes

- O formulário de `/perfil` passa a montar gêneros, plataformas e estilos de
  gameplay a partir de `GET /catalogo/filtros`, com opções selecionáveis em vez
  de áreas de texto livre.
- A API valida cada gênero, plataforma e estilo contra as características do
  catálogo (`genero`, `plataforma`, `tag`) e responde **422** em ProblemDetails,
  sem gravar nada, quando algum valor não existe no catálogo.
- Valores legados fora do catálogo continuam visíveis e removíveis no perfil,
  mas não podem ser recriados depois de removidos.
- **BREAKING**: o campo `experiencias` sai do contrato de `GET/PUT /perfil` e
  do formulário; a experiência permanece apenas como fator adicional do Modo
  Descoberta (RF-13..15). Perfis antigos com experiências deixam de exibi-las e
  o próximo salvamento as remove.
- Os pesos da compatibilidade são revisados: gêneros 40%, plataformas 20%,
  estilos 25% no fluxo padrão; a experiência (15%) só existe no Modo
  Descoberta (TDD DT-07).
- Home e tela de perfil deixam de exibir “Preferências de experiência”.

Escopo rastreável a: RF-05, RF-06, RF-10, RF-13..15; RB-06, RB-10, RB-16;
CA-06, CA-07, CA-12, CA-21, CA-22; TDD DT-07, DT-08, DT-18, TC-01.

## Capabilities

### New Capabilities

Nenhuma — a mudança altera o comportamento do perfil já especificado.

### Modified Capabilities

- `perfil-gamer`: consulta e salvamento passam a usar o vocabulário do catálogo,
  recusam valores fora dele, preservam dados legados removíveis e deixam de
  expor experiências; a interface `/perfil` passa a oferecer opções do
  catálogo em vez de texto livre.

## Impact

- **API**: `GameMatch.Application/Perfil` (`ServicoPerfil`, `PerfilSalvarDto`,
  `PerfilDto`) e injeção de `ICatalogoRepositorio`; `PerfilController` mantém o
  contrato 422/ProblemDetails. Nenhuma migração de banco é necessária
  (`TipoPreferencia.Experiencia` permanece para leitura de dados antigos).
- **Web**: `features/perfil` (formulário com opções e tratamento de legado),
  `features/inicio` (remove experiências), `core/servicos/perfil.service.ts`
  (contrato sem `experiencias`); reutiliza `core/servicos/catalogo.service.ts`
  (`obterFiltros`).
- **Testes**: unitários de `ServicoPerfil` com catálogo fake; integração de
  perfil com catálogo semeado; specs de `perfil` e `inicio` ajustadas.
- **Fora de escopo**: motor de recomendação (F3), novas rotas de catálogo e
  alterações no snapshot/sincronização.

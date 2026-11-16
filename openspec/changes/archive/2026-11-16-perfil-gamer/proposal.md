# Proposal

## Why

A fundação do GameMatch já entrega contas e sessão, mas as recomendações
personalizadas não podem existir sem uma fonte de verdade das preferências do
jogador. Esta change executa a fase **F1 — Perfil gamer** do roadmap do
[`docs/tdd.md`](../../../docs/tdd.md), atendendo aos requisitos **RF-05/RF-06**
e aos critérios **CA-06/CA-07** do [`docs/prd.md`](../../../docs/prd.md), além
da decisão **DT-06** (preferências normalizadas) e do critério técnico
**TC-01**.

## What Changes

- Criar a entidade de domínio `PerfilGamer`, associada 1:1 ao usuário, com
  preferências normalizadas em linhas `(UsuarioId, Tipo, Valor)` e os tipos
  `Genero`, `Plataforma`, `Estilo` e `Experiencia` (DT-06).
- Implementar a regra de completude do perfil: gêneros e plataformas são
  obrigatórios; estilos e experiências são opcionais (RF-05, RB-06).
- Expor `GET /perfil` e `PUT /perfil`, autenticados por JWT, com validação de
  entrada e erros em ProblemDetails (RF-05; DT-05).
- Expor no modelo de leitura um indicador `completo`, derivado da regra de
  completude, para que a interface saiba quando as recomendações estarão
  habilitadas (RF-06; consumido pela F3).
- Criar a rota lazy `/perfil` no frontend em modo de leitura, exibindo o nome
  de exibição derivado do e-mail da sessão e as preferências atuais, com o
  botão “Editar perfil” que revela o formulário de gêneros, plataformas,
  estilos e experiências; após salvar, a tela volta ao modo de leitura com os
  valores atualizados.
- Adicionar à tela de início uma seção autenticada com o resumo do perfil
  (nome de exibição, preferências e indicador de completude), com atalho para
  `/perfil`.
- Manter estados de carregamento/erro/sucesso na tela de perfil e o link de
  acesso no cabeçalho.
- Adicionar migração EF Core para `perfis_gamer` e `perfis_preferencias`, com
  unicidade composta e remoção em cascata ao excluir o usuário.
- Cobrir a mudança com testes unitários (completude, normalização e
  deduplicação), de integração (401, 422, leitura e edição do perfil) e de
  componente (leitura/edição e resumo na home).

**Fora desta change:** cálculo e endpoint de recomendações (F3), catálogo e
snapshot da FreeToGame (F2), favoritos e avaliações (F4). A indisponibilização
de recomendações sem perfil completo (RB-07/CA-13) será consumida na F3 a
partir da regra e do indicador criados aqui.

## Capabilities

### New Capabilities

- `perfil-gamer`: leitura e edição do perfil gamer do usuário autenticado,
  incluindo preferências obrigatórias e opcionais, normalização e a regra que
  determina quando o perfil está completo.

### Modified Capabilities

Nenhuma. O projeto ainda não possui especificações principais em
`openspec/specs/`; esta é a primeira capability de comportamento de produto.

## Impact

- **Backend:** novas entidades e enums em `GameMatch.Domain`, DTOs e caso de
  uso em `GameMatch.Application`, configuração EF e migração em
  `GameMatch.Infrastructure`, `PerfilController` em `GameMatch.Api`.
- **Banco:** duas tabelas novas (`PerfisGamer` e `PerfisPreferencias`) e uma
  migração; nenhuma alteração destrutiva nas tabelas existentes.
- **Frontend:** nova feature `features/perfil` em modo de leitura/edição,
  seção de resumo na tela inicial, serviço em `core/servicos`, rota autenticada
  `/perfil` e ajuste do cabeçalho. O nome de exibição é derivado do e-mail da
  sessão, sem novo campo de conta.
- **Contrato:** novo endpoint `/perfil` (GET/PUT), sem versionamento, seguindo
  DT-05; nenhuma quebra nos endpoints de conta existentes.
- **Testes:** `GameMatch.UnitTests`, `GameMatch.IntegrationTests` e runner do
  Angular CLI, conforme DT-16.

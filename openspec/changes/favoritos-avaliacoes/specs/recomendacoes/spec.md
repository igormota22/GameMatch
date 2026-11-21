# Spec Delta

## ADDED Requirements

### Requirement: Excluir jogos já marcados pelo usuário

O sistema DEVE (MUST) excluir das recomendações os jogos que o usuário já
favoritou ou já avaliou, antes da ordenação e do limite do topo (DT-12,
INV-06), mantendo-os disponíveis no catálogo. Rastreia RF-12, RF-16, RF-17,
DT-12 e CA-16/17.

#### Scenario: Usuário com favoritos e avaliações

- **WHEN** um usuário com favoritos e avaliações consulta as recomendações
- **THEN** nenhum jogo já favoritado ou avaliado aparece na resposta

#### Scenario: Remoção devolve o jogo às recomendações

- **WHEN** o usuário remove um favorito e consulta as recomendações novamente
- **THEN** o jogo volta a ser elegível para aparecer no topo 12

#### Scenario: Usuário sem marcações

- **WHEN** um usuário sem favoritos e sem avaliações consulta as recomendações
- **THEN** o topo 12 é calculado sobre o catálogo completo, sem exclusões

### Requirement: Sinalizar marcações nos cards de recomendação

A aplicação web DEVE (MUST) refletir nos cards de recomendação o estado de
favorito, permitindo favoritar e remover sem recarregar a lista, mantendo a
pontuação e a explicação já exibidas. Rastreia RF-16, RF-11, RNF-07 e CA-16.

#### Scenario: Favoritar a partir da recomendação

- **WHEN** o usuário favorita um jogo recomendado
- **THEN** o botão passa a indicar o estado favoritado e a recomendação permanece visível na sessão atual

#### Scenario: Recarregar após remoção

- **WHEN** o usuário remove um favorito e recarrega as recomendações
- **THEN** o jogo removido volta a ser elegível e pode aparecer na lista

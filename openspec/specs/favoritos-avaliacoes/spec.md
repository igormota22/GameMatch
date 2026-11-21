# favoritos-avaliacoes Specification

## Purpose

Permitir que o jogador marque jogos de interesse e registre sua opinião em
estrelas, de forma privada, única por jogo e sem duplicados, sustentando as
jornadas J-6 e J-7 do PRD e a exclusão de itens já marcados nas recomendações.

## Requirements

### Requirement: Favoritar um jogo sem duplicados

O sistema DEVE (MUST) permitir favoritar um jogo por sessão autenticada,
respondendo **201** na criação e **200 idempotente** quando o par já existe,
sem limite de quantidade; jogo inexistente responde **404** e sessão ausente
**401**. Rastreia RF-16, RB-14, CA-16 e INV-05.

#### Scenario: Primeira favoritação

- **WHEN** o usuário autenticado favorita um jogo ainda não favoritado
- **THEN** a resposta é 201 e o jogo passa a constar na lista de favoritos

#### Scenario: Favoritação repetida

- **WHEN** o usuário favorita novamente um jogo que já está nos favoritos
- **THEN** a resposta é 200 idempotente e nenhum duplicado é criado

#### Scenario: Jogo inexistente

- **WHEN** o usuário favorita um identificador de jogo que não existe
- **THEN** a resposta é 404 e nada é gravado

### Requirement: Remover um jogo dos favoritos

O sistema DEVE (MUST) remover um favorito do próprio usuário, respondendo
**204**; a remoção NÃO DEVE (MUST NOT) afetar a avaliação do mesmo jogo e um
favorito inexistente responde **404**. Rastreia RF-16, RB-14, CA-16 e INV-07.

#### Scenario: Remoção bem-sucedida

- **WHEN** o usuário remove um jogo dos seus favoritos
- **THEN** a resposta é 204 e o jogo deixa de aparecer na lista de favoritos

#### Scenario: Remoção preserva a avaliação

- **WHEN** o usuário remove dos favoritos um jogo que possuía avaliação
- **THEN** a avaliação permanece registrada

#### Scenario: Favorito inexistente

- **WHEN** o usuário remove um favorito que não existe
- **THEN** a resposta é 404

### Requirement: Listar os favoritos do usuário

O sistema DEVE (MUST) listar sob sessão autenticada apenas os favoritos do
usuário identificado pelo token, com paginação opcional, no mesmo formato de
página do catálogo. Rastreia RF-18, RB-14, CA-16 e INV-07.

#### Scenario: Lista paginada

- **WHEN** o usuário consulta seus favoritos com página e tamanho opcionais
- **THEN** a resposta é uma página com os jogos favoritados e a paginação

#### Scenario: Sem favoritos

- **WHEN** o usuário autenticado não possui favoritos
- **THEN** a resposta é uma página vazia

### Requirement: Avaliar um jogo de 1 a 5 estrelas

O sistema DEVE (MUST) aceitar sob sessão autenticada a nota inteira de 1 a 5
para um jogo, criando ou substituindo a avaliação existente (uma por jogo);
nota fora da faixa responde **422**, jogo inexistente **404** e sessão ausente
**401**. Rastreia RF-17, RB-15, CA-17 e INV-04/05.

#### Scenario: Primeira avaliação

- **WHEN** o usuário avalia um jogo sem avaliação anterior com nota válida
- **THEN** a avaliação fica registrada com a nota informada

#### Scenario: Substituição da avaliação

- **WHEN** o usuário envia nova nota para um jogo já avaliado
- **THEN** a avaliação existente é substituída pela nova nota, sem duplicar

#### Scenario: Nota fora da faixa

- **WHEN** o usuário envia nota menor que 1 ou maior que 5
- **THEN** a resposta é 422 e nenhuma avaliação é gravada

### Requirement: Consultar e remover a avaliação

O sistema DEVE (MUST) expor a avaliação do próprio usuário para um jogo
(`GET /avaliacoes/{jogoId}` com a nota, ou **404** quando não existe) e
permitir removê-la com **204**. Rastreia RF-17, RB-15 e CA-17.

#### Scenario: Consulta da avaliação existente

- **WHEN** o usuário consulta a avaliação de um jogo que avaliou
- **THEN** a resposta contém a nota registrada

#### Scenario: Remoção da avaliação

- **WHEN** o usuário remove a avaliação de um jogo
- **THEN** a resposta é 204 e a consulta seguinte responde 404

### Requirement: Proteger favoritos e avaliações do próprio usuário

O sistema DEVE (MUST) derivar o usuário sempre do token de sessão, nunca do
corpo ou da URL, de modo que um usuário não possa ler, criar, alterar ou
remover favoritos e avaliações de outro. Rastreia RF-16, RF-17, INV-07 e
RNF-02.

#### Scenario: Acesso a dados de outro usuário

- **WHEN** um usuário autenticado tenta operar sobre favoritos ou avaliações
  identificados por outro usuário
- **THEN** os dados do outro usuário não são expostos nem alterados

### Requirement: Oferecer favoritos e avaliações na interface web

A aplicação web DEVE (MUST) oferecer botão de favoritar nos cards do catálogo
e das recomendações, com estado refletido, e a rota autenticada `/favoritos`
lista os favoritos com remoção e controle de estrelas 1–5 para avaliar,
alterar e remover a avaliação, com estados vazio e de erro claros. Rastreia
RF-16, RF-17, RF-18, RNF-07, CA-16 e CA-17.

#### Scenario: Favoritar a partir do card

- **WHEN** o usuário aciona favoritar em um card do catálogo ou das recomendações
- **THEN** o estado do botão reflete que o jogo está favoritado e a remoção é possível

#### Scenario: Tela de favoritos vazia

- **WHEN** o usuário abre `/favoritos` sem nenhum favorito
- **THEN** a tela exibe estado vazio com sugestão de explorar o catálogo

#### Scenario: Avaliar na tela de favoritos

- **WHEN** o usuário seleciona de 1 a 5 estrelas em um favorito
- **THEN** a avaliação é registrada e a nota selecionada permanece visível

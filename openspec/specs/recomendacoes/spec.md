# recomendacoes Specification

## Purpose

Converter as preferências do perfil gamer e o catálogo sincronizado em
recomendações explicáveis, ordenadas e personalizadas, incluindo o Modo
Descoberta que cruza uma opção de experiência escolhida no momento da consulta
com o perfil do usuário.

## Requirements

### Requirement: Calcular a pontuação de compatibilidade

O sistema DEVE (MUST) calcular, para cada jogo do catálogo, uma pontuação de
compatibilidade de 0 a 100 com o perfil do usuário, usando os pesos definidos
no TDD (DT-07): gêneros 40%, plataformas 20% e estilos de gameplay 25%. O fator
de um grupo sem itens no perfil DEVE ser ignorado, com renormalização
proporcional dos pesos entre os fatores definidos. Rastreia RF-10, RB-10 e
CA-11.

#### Scenario: Perfil com gêneros, plataformas e estilos

- **WHEN** um usuário com gêneros, plataformas e estilos preenchidos consulta as recomendações
- **THEN** cada jogo recebe uma pontuação inteira entre 0 e 100 calculada a partir dos três fatores

#### Scenario: Perfil sem estilos de gameplay

- **WHEN** o perfil possui gêneros e plataformas, mas nenhuma estilização de gameplay
- **THEN** o fator de estilos é ignorado e os pesos de gênero e plataforma são renormalizados entre si

#### Scenario: Jogo sem nenhuma correspondência

- **WHEN** nenhum gênero, plataforma ou estilo do jogo corresponde ao perfil
- **THEN** a pontuação do jogo é zero

### Requirement: Ordenar e limitar as recomendações

O sistema DEVE (MUST) entregar as recomendações ordenadas da maior para a
menor pontuação e limitar a resposta ao topo de itens definido em configuração
(DT-14, valor atual: 12). Rastreia RF-12, RB-10 e CA-11.

#### Scenario: Catálogo maior que o limite

- **WHEN** o catálogo possui mais jogos do que o limite configurado
- **THEN** a resposta contém apenas os melhores jogos, ordenados por pontuação decrescente

#### Scenario: Catálogo menor que o limite

- **WHEN** o catálogo possui menos jogos que o limite configurado
- **THEN** a resposta contém todos os jogos, ordenados por pontuação decrescente

### Requirement: Explicar os fatores da pontuação

O sistema DEVE (MUST) retornar, em cada recomendação, os fatores que
contribuíram para a pontuação com rótulo, contribuição numérica e lista de
itens correspondidos, de modo que a soma das contribuições corresponda à
pontuação (com tolerância de ±1 por arredondamento), além da breve descrição
do jogo para contextualizar a sugestão. Rastreia RF-11, RB-11 e CA-12.

#### Scenario: Recomendação exibida

- **WHEN** uma recomendação é retornada ao cliente
- **THEN** ela apresenta a pontuação, a breve descrição do jogo e cada fator com sua contribuição e as correspondências que a originaram

#### Scenario: Fator sem correspondências

- **WHEN** um fator não possui nenhuma correspondência com o jogo
- **THEN** o fator é apresentado com contribuição zero

### Requirement: Exigir sessão e perfil completo para recomendar

O sistema DEVE (MUST) exigir sessão autenticada para servir recomendações e
rejeitar a consulta com **409** em ProblemDetails identificado como
`perfil-incompleto` enquanto o perfil não possuir gêneros e plataformas,
sem calcular nem retornar itens. Rastreia RF-13, RB-07 e CA-13.

#### Scenario: Acesso sem sessão

- **WHEN** uma requisição sem token válido consulta as recomendações
- **THEN** o sistema responde 401 com ProblemDetails e não expõe recomendações

#### Scenario: Perfil sem preferências mínimas

- **WHEN** um usuário autenticado sem gênero ou sem plataforma consulta as recomendações
- **THEN** o sistema responde 409 com ProblemDetails identificado como `perfil-incompleto` e não retorna itens

#### Scenario: Perfil completo após configuração

- **WHEN** o usuário preenche gêneros e plataformas e consulta novamente
- **THEN** o sistema retorna a lista de recomendações normalmente

### Requirement: Oferecer as opções do Modo Descoberta

O sistema DEVE (MUST) expor as opções predefinidas de experiência do Modo
Descoberta (chave e rótulo) a partir da configuração `Descoberta:Opcoes`
(DT-09), sob sessão autenticada. Rastreia RF-14, RB-13 e CA-14.

#### Scenario: Consulta das opções

- **WHEN** um usuário autenticado consulta as opções do Modo Descoberta
- **THEN** o sistema retorna a lista de opções com chave e rótulo na ordem configurada

#### Scenario: Acesso sem sessão

- **WHEN** uma requisição sem token válida consulta as opções
- **THEN** o sistema responde 401 com ProblemDetails

### Requirement: Combinar a experiência escolhida com o perfil

O sistema DEVE (MUST) aceitar a chave opcional `experiencia` em
`GET /recomendacoes`, incluindo o mapeamento da opção como fator adicional de
15% na pontuação (DT-07/DT-09) e o exibindo na explicação; chave desconhecida
DEVE responder **404** em ProblemDetails. Sem preferências mínimas aplica-se o
aviso de perfil incompleto. Rastreia RF-15, RB-11, RB-13 e CA-15.

#### Scenario: Opção válida enviada

- **WHEN** o usuário envia uma `experiencia` existente com perfil completo
- **THEN** o sistema retorna recomendações cuja explicação inclui o fator de experiência com sua contribuição

#### Scenario: Opção desconhecida enviada

- **WHEN** o usuário envia uma `experiencia` que não existe na configuração
- **THEN** o sistema responde 404 com ProblemDetails e não calcula recomendações

#### Scenario: Perfil incompleto no Modo Descoberta

- **WHEN** um usuário sem gêneros ou sem plataformas consulta com `experiencia`
- **THEN** o sistema responde 409 com ProblemDetails identificado como `perfil-incompleto`

### Requirement: Exibir recomendações e Modo Descoberta no frontend

A aplicação web DEVE (MUST) oferecer a rota autenticada `/recomendacoes`
exibindo a lista de recomendações com pontuação e fatores explicados, o seletor
das opções do Modo Descoberta e um CTA para `/perfil` quando a API responder
`perfil-incompleto`. Rastreia RF-10, RF-11, RF-13, RF-14, RF-15, RNF-07 e
CA-11, CA-12, CA-13, CA-14, CA-15.

#### Scenario: Acesso com perfil completo

- **WHEN** um usuário autenticado com perfil completo acessa `/recomendacoes`
- **THEN** a tela exibe as recomendações ordenadas com a pontuação e os fatores visíveis

#### Scenario: Descrição do jogo na explicação

- **WHEN** um card de recomendação é exibido
- **THEN** a seção "Por que este jogo?" apresenta a breve descrição do jogo, em português (PT-BR), logo acima dos fatores

#### Scenario: Atalho para recomendações na home

- **WHEN** um usuário autenticado acessa a tela inicial
- **THEN** a seção "Seu perfil" oferece um atalho "Ver recomendações" para `/recomendacoes`

#### Scenario: Perfil incompleto

- **WHEN** um usuário autenticado sem preferências mínimas acessa `/recomendacoes`
- **THEN** a tela exibe o aviso de perfil incompleto com CTA para `/perfil` em vez da lista

#### Scenario: Ativar o Modo Descoberta

- **WHEN** o usuário escolhe uma opção de experiência no seletor
- **THEN** a lista é atualizada e cada recomendação passa a mostrar também o fator de experiência

#### Scenario: Acesso sem sessão

- **WHEN** um visitante acessa `/recomendacoes`
- **THEN** o sistema o redireciona para `/entrar` preservando o destino `/recomendacoes`

#### Scenario: Falha ao carregar recomendações

- **WHEN** a leitura das recomendações falha por indisponibilidade da API
- **THEN** a tela exibe mensagem de erro em português e mantém a navegação utilizável

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

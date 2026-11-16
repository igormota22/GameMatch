# Spec Delta

## Purpose

Permitir que cada jogador autenticado mantenha as preferências que alimentam as
recomendações do GameMatch e que o sistema determine quando o perfil está
completo o suficiente para recomendá-las. A aplicação web deve tornar esse
perfil visível e editável, inclusive com um resumo na tela de início.

## ADDED Requirements

### Requirement: Consultar o próprio perfil gamer

O sistema DEVE (MUST) disponibilizar ao usuário autenticado a leitura do próprio
perfil gamer, com gêneros, plataformas, estilos de gameplay, experiências e
indicador de completude. Rastreia RF-05.

#### Scenario: Perfil existente

- **WHEN** um usuário autenticado com perfil salvo consulta o perfil
- **THEN** o sistema retorna as preferências salvas e o indicador de completude correspondente

#### Scenario: Perfil ainda não configurado

- **WHEN** um usuário autenticado sem perfil consulta o perfil
- **THEN** o sistema responde com listas vazias e indicador de perfil incompleto, sem criar registro

#### Scenario: Acesso sem sessão

- **WHEN** uma requisição sem token válido consulta o perfil
- **THEN** o sistema responde 401 com ProblemDetails e não expõe dados de perfil

### Requirement: Salvar ou editar o perfil gamer

O sistema DEVE (MUST) criar ou substituir o perfil do usuário autenticado a partir das
preferências enviadas, aceitando gêneros e plataformas como obrigatórios e
estilos e experiências como opcionais. Rastreia RF-05, RB-05 e RB-06.

#### Scenario: Criação com preferências obrigatórias e opcionais

- **WHEN** um usuário autenticado envia ao menos um gênero, ao menos uma plataforma, estilos e experiências
- **THEN** o sistema salva o perfil e passa a retorná-lo na consulta

#### Scenario: Criação apenas com preferências obrigatórias

- **WHEN** um usuário autenticado envia gêneros e plataformas e deixa estilos e experiências vazios
- **THEN** o sistema salva o perfil com as listas opcionais vazias

#### Scenario: Edição de perfil existente

- **WHEN** um usuário autenticado envia novas preferências para um perfil já salvo
- **THEN** o sistema substitui as preferências anteriores e a próxima consulta reflete os novos valores

#### Scenario: Valores repetidos ou com espaços

- **WHEN** o usuário envia o mesmo valor mais de uma vez ou com espaços nas extremidades
- **THEN** o sistema normaliza os valores e persiste cada preferência uma única vez

### Requirement: Rejeitar perfil sem preferências obrigatórias

O sistema DEVE (MUST) rejeitar o salvamento quando gêneros ou plataformas estiverem
ausentes ou vazios, preservando o perfil anterior. Rastreia RF-05, RB-06 e
CA-06.

#### Scenario: Gêneros ausentes

- **WHEN** o usuário tenta salvar sem ao menos um gênero
- **THEN** o sistema responde 422 com ProblemDetails e não altera o perfil

#### Scenario: Plataformas ausentes

- **WHEN** o usuário tenta salvar sem ao menos uma plataforma
- **THEN** o sistema responde 422 com ProblemDetails e não altera o perfil

#### Scenario: Falha preserva perfil válido

- **WHEN** um usuário com perfil completo envia uma alteração inválida
- **THEN** o sistema rejeita a alteração e mantém o perfil anterior consultável

### Requirement: Determinar a completude do perfil

O sistema DEVE (MUST) considerar o perfil completo somente quando houver ao menos um
gênero e ao menos uma plataforma; estilos e experiências não alteram essa
condição. Rastreia RF-06 e RB-07.

#### Scenario: Perfil completo

- **WHEN** o perfil possui ao menos um gênero e ao menos uma plataforma
- **THEN** o sistema indica o perfil como completo

#### Scenario: Perfil incompleto

- **WHEN** o perfil não possui gênero ou não possui plataforma
- **THEN** o sistema indica o perfil como incompleto

#### Scenario: Tentativa de remover item obrigatório

- **WHEN** uma edição remove todos os gêneros ou todas as plataformas de um perfil completo
- **THEN** a edição é rejeitada pela validação e o perfil permanece completo

### Requirement: Isolar o perfil por usuário

O sistema DEVE (MUST) vincular cada perfil ao usuário autenticado e impedir que um
usuário leia ou altere o perfil de outro. Rastreia RF-05, RNF-02 e INV-07.

#### Scenario: Identidade vem da sessão

- **WHEN** um usuário autenticado consulta ou salva o perfil
- **THEN** o sistema usa exclusivamente a identidade do token, sem aceitar identificador de usuário na requisição

#### Scenario: Um único perfil por usuário

- **WHEN** o mesmo usuário salva o perfil mais de uma vez
- **THEN** o sistema mantém um único perfil atualizado, sem duplicar o registro

### Requirement: Exibir e editar o perfil gamer no frontend

A aplicação web DEVE (MUST) oferecer a rota autenticada `/perfil` em modo de
leitura, exibindo o nome de exibição derivado do endereço de e-mail da sessão
(parte anterior a `@`, com inicial maiúscula) e as preferências atuais, com uma
ação “Editar perfil” que revela o formulário acessível. O salvamento bem-sucedido
retorna ao modo de leitura com os valores atualizados. Rastreia RF-04, RF-05 e
RNF-07.

#### Scenario: Acesso autenticado com perfil salvo

- **WHEN** um usuário autenticado com perfil salvo acessa `/perfil`
- **THEN** a tela exibe o nome de exibição, as preferências atuais e o indicador de completude em modo de leitura, com a ação “Editar perfil”

#### Scenario: Perfil ainda não configurado

- **WHEN** um usuário autenticado sem preferências salvas acessa `/perfil`
- **THEN** a tela exibe um estado vazio com orientação e a ação que abre o formulário para configurar o perfil

#### Scenario: Abrir a edição

- **WHEN** o usuário aciona “Editar perfil”
- **THEN** os campos de gêneros, plataformas, estilos e experiências são exibidos preenchidos com os valores atuais

#### Scenario: Salvar a edição

- **WHEN** o usuário salva preferências válidas
- **THEN** a tela volta ao modo de leitura exibindo as preferências atualizadas e a confirmação de sucesso

#### Scenario: Acesso sem sessão

- **WHEN** um visitante acessa `/perfil`
- **THEN** o sistema o redireciona para `/entrar` preservando o destino `/perfil`

#### Scenario: Erro ao salvar

- **WHEN** o salvamento falha por validação ou indisponibilidade da API
- **THEN** a tela permanece em edição, exibe a mensagem de erro em português e mantém os dados preenchidos para correção

### Requirement: Exibir o resumo do perfil na tela de início

A aplicação web DEVE (MUST) exibir, para o usuário autenticado, uma seção na
tela de início com o nome de exibição derivado do endereço de e-mail da sessão
(parte anterior a `@`, com inicial maiúscula), as preferências atuais e o
indicador de completude, além de um atalho para `/perfil`. Visitantes não DEVEM
ver essa seção. Rastreia RF-04, RF-05 e RF-06.

#### Scenario: Usuário autenticado com perfil completo

- **WHEN** um usuário autenticado com perfil completo acessa a tela de início
- **THEN** a seção exibe o nome de exibição, as preferências e o indicador de perfil completo, com atalho para `/perfil`

#### Scenario: Usuário autenticado sem preferências mínimas

- **WHEN** um usuário autenticado sem perfil completo acessa a tela de início
- **THEN** a seção exibe o estado incompleto e orienta a configurar o perfil em `/perfil`

#### Scenario: Visitante

- **WHEN** um visitante acessa a tela de início
- **THEN** a seção de resumo do perfil não é exibida

#### Scenario: Falha ao carregar o perfil na home

- **WHEN** a leitura do perfil falha na tela de início
- **THEN** o restante da home continua disponível e a seção exibe uma mensagem em português com atalho para abrir `/perfil`

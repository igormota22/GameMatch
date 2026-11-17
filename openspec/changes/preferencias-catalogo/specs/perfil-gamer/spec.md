# Spec Delta

## MODIFIED Requirements

### Requirement: Consultar o próprio perfil gamer

O sistema DEVE (MUST) disponibilizar ao usuário autenticado a leitura do próprio
perfil gamer, com gêneros, plataformas, estilos de gameplay e indicador de
completude, preservando valores legados que não existam mais no catálogo.
Rastreia RF-05 e RB-16.

#### Scenario: Perfil existente

- **WHEN** um usuário autenticado com perfil salvo consulta o perfil
- **THEN** o sistema retorna as preferências salvas e o indicador de completude correspondente

#### Scenario: Perfil com valores legados

- **WHEN** o perfil possui gêneros, plataformas ou estilos que não existem no catálogo atual
- **THEN** o sistema retorna esses valores como salvos, sem erro e sem removê-los automaticamente

#### Scenario: Perfil ainda não configurado

- **WHEN** um usuário autenticado sem perfil consulta o perfil
- **THEN** o sistema responde com listas vazias e indicador de perfil incompleto, sem criar registro

#### Scenario: Acesso sem sessão

- **WHEN** uma requisição sem token válido consulta o perfil
- **THEN** o sistema responde 401 com ProblemDetails e não expõe dados de perfil

### Requirement: Salvar ou editar o perfil gamer

O sistema DEVE (MUST) criar ou substituir o perfil do usuário autenticado a partir
das preferências enviadas, aceitando gêneros e plataformas como obrigatórios e
estilos de gameplay como opcionais, entre as opções do catálogo. Rastreia RF-05,
RB-05, RB-06 e RB-16.

#### Scenario: Criação com preferências obrigatórias e opcionais

- **WHEN** um usuário autenticado envia ao menos um gênero, ao menos uma plataforma e estilos existentes no catálogo
- **THEN** o sistema salva o perfil e passa a retorná-lo na consulta

#### Scenario: Criação apenas com preferências obrigatórias

- **WHEN** um usuário autenticado envia gêneros e plataformas do catálogo e deixa estilos vazios
- **THEN** o sistema salva o perfil com a lista de estilos vazia

#### Scenario: Edição de perfil existente

- **WHEN** um usuário autenticado envia novas preferências para um perfil já salvo
- **THEN** o sistema substitui as preferências anteriores e a próxima consulta reflete os novos valores

#### Scenario: Valores repetidos ou com espaços

- **WHEN** o usuário envia o mesmo valor mais de uma vez ou com espaços nas extremidades
- **THEN** o sistema normaliza os valores e persiste cada preferência uma única vez

#### Scenario: Campo de experiência ignorado

- **WHEN** o usuário envia um perfil contendo o campo `experiencias`
- **THEN** o sistema ignora o campo, não cria preferências de experiência e não falha o salvamento por causa dele

#### Scenario: Experiências antigas removidas no salvamento

- **WHEN** um perfil legado com preferências de experiência é salvo novamente
- **THEN** o sistema substitui as preferências e o perfil deixa de possuir experiências

### Requirement: Determinar a completude do perfil

O sistema DEVE (MUST) considerar o perfil completo somente quando houver ao menos um
gênero e ao menos uma plataforma; estilos não alteram essa condição. Rastreia
RF-06 e RB-07.

#### Scenario: Perfil completo

- **WHEN** o perfil possui ao menos um gênero e ao menos uma plataforma
- **THEN** o sistema indica o perfil como completo

#### Scenario: Perfil incompleto

- **WHEN** o perfil não possui gênero ou não possui plataforma
- **THEN** o sistema indica o perfil como incompleto

#### Scenario: Tentativa de remover item obrigatório

- **WHEN** uma edição remove todos os gêneros ou todas as plataformas de um perfil completo
- **THEN** a edição é rejeitada pela validação e o perfil permanece completo

### Requirement: Exibir e editar o perfil gamer no frontend

A aplicação web DEVE (MUST) oferecer a rota autenticada `/perfil` em modo de
leitura, exibindo o nome de exibição derivado do endereço de e-mail da sessão
(parte anterior a `@`, com inicial maiúscula) e as preferências atuais, com uma
ação “Editar perfil” que revela o formulário acessível com opções do catálogo. O
salvamento bem-sucedido retorna ao modo de leitura com os valores atualizados.
Rastreia RF-04, RF-05, RB-16 e RNF-07.

#### Scenario: Acesso autenticado com perfil salvo

- **WHEN** um usuário autenticado com perfil salvo acessa `/perfil`
- **THEN** a tela exibe o nome de exibição, as preferências atuais e o indicador de completude em modo de leitura, com a ação “Editar perfil”

#### Scenario: Perfil ainda não configurado

- **WHEN** um usuário autenticado sem preferências salvas acessa `/perfil`
- **THEN** a tela exibe um estado vazio com orientação e a ação que abre o formulário para configurar o perfil

#### Scenario: Abrir a edição

- **WHEN** o usuário aciona “Editar perfil”
- **THEN** os campos de gêneros, plataformas e estilos são exibidos como opções obtidas do catálogo, já marcadas com os valores atuais, sem campos de texto livre

#### Scenario: Valores legados na edição

- **WHEN** o perfil possui valores que não existem no catálogo atual
- **THEN** a edição os exibe em um grupo identificado como fora do catálogo, permitindo desmarcá-los para removê-los

#### Scenario: Catálogo indisponível na edição

- **WHEN** as opções do catálogo não podem ser carregadas ou vêm vazias
- **THEN** a tela informa que não é possível editar as preferências, oferece nova tentativa e impede o salvamento

#### Scenario: Salvar a edição

- **WHEN** o usuário salva preferências válidas
- **THEN** a tela volta ao modo de leitura exibindo as preferências atualizadas e a confirmação de sucesso

#### Scenario: Acesso sem sessão

- **WHEN** um visitante acessa `/perfil`
- **THEN** o sistema o redireciona para `/entrar` preservando o destino `/perfil`

#### Scenario: Erro ao salvar

- **WHEN** o salvamento falha por validação ou indisponibilidade da API
- **THEN** a tela permanece em edição, exibe a mensagem de erro em português e mantém os dados preenchidos para correção

## ADDED Requirements

### Requirement: Exigir o vocabulário do catálogo nas preferências

A API DEVE (MUST) aceitar apenas gêneros, plataformas e estilos existentes no
catálogo sincronizado, respondendo 422 em ProblemDetails sem gravar nada quando
algum valor não existir. A comparação DEVE ignorar diferenças de caixa e o valor
persistido DEVE usar a grafia do catálogo. Rastreia RF-05, RB-16 e CA-21.

#### Scenario: Valores existentes no catálogo

- **WHEN** o usuário envia gêneros, plataformas e estilos presentes no catálogo
- **THEN** o sistema salva as preferências usando a grafia do catálogo

#### Scenario: Gênero fora do catálogo

- **WHEN** o usuário envia um gênero que não existe no catálogo
- **THEN** o sistema responde 422 com ProblemDetails indicando `generos` e não altera o perfil

#### Scenario: Plataforma fora do catálogo

- **WHEN** o usuário envia uma plataforma que não existe no catálogo
- **THEN** o sistema responde 422 com ProblemDetails indicando `plataformas` e não altera o perfil

#### Scenario: Estilo fora do catálogo

- **WHEN** o usuário envia um estilo que não existe como tag do catálogo
- **THEN** o sistema responde 422 com ProblemDetails indicando `estilos` e não altera o perfil

#### Scenario: Caixa diferente da grafia do catálogo

- **WHEN** o usuário envia um valor do catálogo com caixa diferente
- **THEN** o sistema o aceita e persiste a grafia do catálogo

#### Scenario: Catálogo vazio

- **WHEN** o catálogo não possui características e o usuário tenta salvar preferências
- **THEN** o sistema responde 422 com ProblemDetails e não grava o perfil

### Requirement: Preservar valores legados fora do catálogo

O sistema DEVE (MUST) permitir que valores já pertencentes ao perfil sejam
reenviados mesmo sem correspondência no catálogo, impedindo que sejam removidos
e recriados depois. Rastreia RF-05, RB-16 e CA-22.

#### Scenario: Reenvio de valor legado

- **WHEN** o usuário salva o perfil mantendo um valor que já estava salvo, mas não existe no catálogo
- **THEN** o sistema aceita e mantém o valor como estava

#### Scenario: Remoção de valor legado

- **WHEN** o usuário remove um valor legado e salva o perfil sem ele
- **THEN** a próxima consulta não retorna mais esse valor

#### Scenario: Recriação de valor legado removido

- **WHEN** o usuário tenta salvar um valor fora do catálogo que não pertence mais ao perfil
- **THEN** o sistema responde 422 com ProblemDetails e não grava o valor

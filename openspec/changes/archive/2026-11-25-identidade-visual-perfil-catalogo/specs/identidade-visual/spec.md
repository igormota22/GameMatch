# Spec Delta

## ADDED Requirements

### Requirement: Página de perfil gamer com a identidade da locadora
A página `/perfil` SHALL exibir a identidade da locadora preservando o nome de exibição, as preferências atuais, o indicador de completude, os estados de carregamento/erro/sucesso, a ação `Minha conta` e o formulário de edição com opções do catálogo, incluindo o grupo de valores legados fora do catálogo (RF-19; RF-04; RF-05; RF-06; CA-06; CA-22).

#### Scenario: Usuário autenticado abre o perfil em modo de leitura
- **WHEN** um usuário autenticado com perfil salvo acessa `/perfil`
- **THEN** a página exibe a identidade da locadora e os textos `Perfil gamer`, `Jogador`, o nome de exibição, `Gêneros favoritos`, `Plataformas`, `Estilos de gameplay`, `Perfil completo` ou `Perfil incompleto`, `Minha conta` e a ação `Editar perfil`, sem campos de formulário visíveis

#### Scenario: Perfil ainda não configurado
- **WHEN** um usuário autenticado sem preferências acessa `/perfil`
- **THEN** a página exibe o estado vazio com a orientação de configuração, os valores `Não informado` e a ação `Editar perfil`, mantendo a identidade da locadora

#### Scenario: Edição das preferências
- **WHEN** o usuário aciona `Editar perfil`
- **THEN** o formulário exibe os grupos `Gêneros favoritos *`, `Plataformas *` e `Estilos de gameplay` com opções marcadas conforme o perfil, os textos de apoio e as ações `Cancelar` e `Salvar perfil`, mantendo os rótulos acessíveis e os valores legados no grupo `Fora do catálogo (legado)`

#### Scenario: Validação local e falha ao salvar
- **WHEN** o usuário tenta salvar sem os obrigatórios ou a API rejeita o salvamento
- **THEN** as mensagens `Informe ao menos um gênero.`, `Informe ao menos uma plataforma.` ou a mensagem de erro retornada pela API são exibidas com `role="alert"`, o formulário permanece preenchido e a identidade da locadora é mantida

#### Scenario: Salvamento bem-sucedido
- **WHEN** o usuário salva preferências válidas
- **THEN** a página volta ao modo de leitura com a confirmação `Perfil salvo com sucesso.`, as preferências atualizadas e o indicador de completude recalculado

#### Scenario: Falha ao carregar o perfil ou o catálogo de opções
- **WHEN** a carga do perfil ou das opções do catálogo falha
- **THEN** a página exibe a mensagem em português com a ação `Tentar novamente` e, no caso do catálogo, mantém o salvamento bloqueado até a recuperação

#### Scenario: Acesso à conta pelo perfil
- **WHEN** o usuário autenticado aciona a ação `Minha conta`
- **THEN** ele é levado a `/conta`, preservando a proteção por sessão

### Requirement: Página de catálogo com a identidade da locadora
A página `/catalogo` SHALL exibir a identidade da locadora preservando a busca por título, os filtros combinados de gênero, plataforma e tag, a contagem e a paginação de resultados, os cards com imagem, título, gêneros, plataformas e desenvolvedor, o botão de favorito e a atribuição à FreeToGame (RF-19; RF-07; RF-08; RF-09; CA-08; CA-09; CA-10; RNF-03).

#### Scenario: Catálogo com resultados
- **WHEN** um usuário autenticado abre `/catalogo` com jogos sincronizados
- **THEN** a página exibe a identidade da locadora, os textos `Catálogo`, `Jogos gratuitos para descobrir`, `Buscar por título`, `Gênero`, `Plataforma`, `Tag`, `Jogos do catálogo` e a contagem, com cards contendo capa, título, gêneros, plataformas, desenvolvedor e o botão `Favoritar`/`Favoritado`

#### Scenario: Busca e filtros
- **WHEN** o usuário pesquisa por título ou altera gênero, plataforma ou tag
- **THEN** a listagem é atualizada a partir da primeira página, mantendo os rótulos dos campos e a contagem anunciada em `aria-live="polite"`

#### Scenario: Paginação
- **WHEN** o catálogo tem mais de uma página
- **THEN** a navegação exibe `Anterior`, `Página X de Y` e `Próxima`, com os estados desabilitados preservados e os botões acionáveis por teclado

#### Scenario: Busca sem resultados
- **WHEN** a busca ou os filtros aplicados não retornam jogos
- **THEN** a página exibe o estado vazio com a ação `Limpar filtros`, que restaura a listagem completa

#### Scenario: Catálogo não sincronizado
- **WHEN** o catálogo não possui jogos e não há filtros aplicados
- **THEN** a página exibe o estado vazio informando que não há jogos no catálogo, sem oferecer `Limpar filtros`

#### Scenario: Falha da API
- **WHEN** a consulta do catálogo ou a ação de favorito falha
- **THEN** a mensagem de erro em português é exibida com `role="alert"`, a navegação do aplicativo permanece disponível e, no caso da listagem, a ação `Tentar novamente` é oferecida

#### Scenario: Atribuição FreeToGame
- **WHEN** a página do catálogo é exibida
- **THEN** o texto `Dados fornecidos por` e o link ativo para `FreeToGame.com` permanecem visíveis e clicáveis

### Requirement: Acessibilidade e responsividade de perfil e catálogo
As páginas `/perfil` e `/catalogo` SHALL manter rótulos associados aos campos, foco visível, navegação por teclado, estados anunciáveis por tecnologia assistiva, contraste AA, layout responsivo sem rolagem horizontal e interações que respeitam `prefers-reduced-motion` (RF-19; RNF-07; RNF-08; CA-23).

#### Scenario: Navegação por teclado
- **WHEN** o usuário percorre o perfil ou o catálogo apenas com Tab
- **THEN** cada campo, opção, botão e link recebe foco visível, na ordem do conteúdo, e as ações permanecem acionáveis sem mouse

#### Scenario: Layout em viewport móvel e desktop
- **WHEN** as duas páginas são exibidas em 390px e 1440px
- **THEN** o conteúdo se adapta sem cortes, sobreposições ou rolagem horizontal e o texto permanece legível

#### Scenario: Movimento reduzido
- **WHEN** o sistema operacional indica `prefers-reduced-motion`
- **THEN** animações decorativas de cards, etiquetas e botões são desativadas ou reduzidas sem afetar o uso das páginas

#### Scenario: Estados dinâmicos anunciados
- **WHEN** a contagem do catálogo, uma mensagem de erro/sucesso ou o estado de carregamento muda
- **THEN** a mudança é exposta em elementos com `role="status"` ou `role="alert"` e os atributos `aria-live`, `aria-invalid` e `aria-pressed` existentes são preservados

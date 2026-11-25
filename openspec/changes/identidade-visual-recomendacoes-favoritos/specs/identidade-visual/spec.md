# Spec Delta

## ADDED Requirements

### Requirement: Página de recomendações com a identidade da locadora
A página `/recomendacoes` SHALL exibir a identidade da locadora preservando o seletor do Modo Descoberta, a lista de recomendações com pontuação, descrição, fatores explicados e correspondências, o botão de favorito e os estados de carregamento, perfil incompleto, erro e vazio (RF-19; RF-10; RF-11; RF-13; RF-14; RF-15; RF-16; CA-11; CA-12; CA-13; CA-14; CA-15; CA-16).

#### Scenario: Recomendações com perfil completo
- **WHEN** um usuário autenticado com perfil completo acessa `/recomendacoes`
- **THEN** a página exibe a identidade da locadora, os textos `Recomendações`, `Jogos feitos para o seu perfil`, `Cada sugestão explica por que combinou com as suas preferências.` e `Modo Descoberta`, com cards contendo capa, título, desenvolvedor, gêneros, a pontuação em porcentagem, a seção `Por que este jogo?` e o botão `Favoritar`/`Favoritado`

#### Scenario: Explicação por fatores
- **WHEN** um card de recomendação é exibido
- **THEN** a seção `Por que este jogo?` apresenta a descrição do jogo e, para cada fator, o rótulo, a contribuição em `pontos` e as correspondências, ou `Sem correspondência` quando não houver, mantendo a identidade da locadora

#### Scenario: Modo Descoberta
- **WHEN** o usuário escolhe uma opção no seletor `#experiencia`
- **THEN** a lista é recarregada com a chave escolhida e cada recomendação passa a exibir o fator `Experiência` com sua contribuição, preservando o rótulo acessível do campo

#### Scenario: Perfil incompleto
- **WHEN** a API responde `perfil-incompleto`
- **THEN** a página exibe o aviso `Perfil incompleto` com a orientação `Informe gêneros e plataformas no perfil para receber recomendações.` e o link `Configurar perfil` para `/perfil`, sem listar recomendações

#### Scenario: Falha ao carregar recomendações
- **WHEN** a leitura das recomendações falha
- **THEN** a mensagem de erro em português é exibida com `role="alert"` e a ação `Tentar novamente` permanece disponível e acionável

#### Scenario: Nenhuma recomendação disponível
- **WHEN** a API retorna uma lista vazia para o perfil
- **THEN** a página exibe `Nenhuma recomendação disponível para o seu perfil no momento.` em um painel com a identidade da locadora, sem cards

#### Scenario: Favoritar a partir da recomendação
- **WHEN** o usuário aciona o botão de favorito em um card de recomendação
- **THEN** o estado `aria-pressed` e o texto `Favoritar`/`Favoritado` são atualizados sem recarregar a lista, mantendo a pontuação e a explicação já exibidas, e a falha da API é anunciada em português

### Requirement: Página de favoritos com a identidade da locadora
A página `/favoritos` SHALL exibir a identidade da locadora preservando a lista paginada de jogos marcados, a contagem, a remoção de favoritos, o controle de estrelas de 1 a 5 para avaliar, alterar e remover a avaliação, e os estados de carregamento, vazio e erro (RF-19; RF-16; RF-17; RF-18; CA-16; CA-17).

#### Scenario: Favoritos com resultados
- **WHEN** um usuário autenticado com favoritos acessa `/favoritos`
- **THEN** a página exibe a identidade da locadora, os textos `Favoritos`, `Seus jogos marcados`, `Lista de favoritos`, a contagem e a paginação, com cards contendo capa, título, desenvolvedor, gêneros, a seção `Sua avaliação` e a ação `Remover dos favoritos`

#### Scenario: Avaliar com estrelas
- **WHEN** o usuário seleciona uma nota de 1 a 5 em um favorito
- **THEN** a nota é registrada, as estrelas até a nota ficam preenchidas com `aria-pressed` correspondente, a ação `Remover avaliação` passa a ser exibida e a identidade da locadora é mantida

#### Scenario: Remover a avaliação
- **WHEN** o usuário aciona `Remover avaliação`
- **THEN** as estrelas voltam ao estado vazio, a ação `Remover avaliação` deixa de ser exibida e a remoção é persistida pela API

#### Scenario: Remover um favorito
- **WHEN** o usuário aciona `Remover dos favoritos` em um card
- **THEN** o favorito é removido, a lista é recarregada e, se a página ficar vazia, a página anterior é exibida, mantendo a identidade da locadora

#### Scenario: Nenhum favorito
- **WHEN** o usuário abre `/favoritos` sem nenhum favorito
- **THEN** a página exibe `Você ainda não tem jogos favoritos.` e a orientação de explorar o catálogo com o link `Explorar catálogo` para `/catalogo`

#### Scenario: Falha ao carregar ou agir sobre favoritos
- **WHEN** a leitura da lista ou uma ação de favorito/avaliação falha
- **THEN** a mensagem de erro em português é exibida com `role="alert"` e, no caso da listagem, a ação `Tentar novamente` é oferecida

#### Scenario: Paginação dos favoritos
- **WHEN** o usuário tem favoritos em mais de uma página
- **THEN** a navegação exibe `Anterior`, `Página X de Y` e `Próxima`, com os estados desabilitados preservados e os botões acionáveis por teclado

### Requirement: Acessibilidade e responsividade de recomendações e favoritos
As páginas `/recomendacoes` e `/favoritos` SHALL manter rótulos associados aos controles, foco visível, navegação por teclado, estados anunciáveis por tecnologia assistiva, contraste AA, layout responsivo sem rolagem horizontal e interações que respeitam `prefers-reduced-motion` (RF-19; RNF-07; RNF-08; CA-23).

#### Scenario: Navegação por teclado
- **WHEN** o usuário percorre as duas páginas apenas com Tab
- **THEN** o seletor do Modo Descoberta, os botões de favorito, as estrelas, as ações de remoção e a paginação recebem foco visível na ordem do conteúdo e permanecem acionáveis sem mouse

#### Scenario: Layout em viewport móvel e desktop
- **WHEN** as duas páginas são exibidas em 390px e 1440px
- **THEN** o conteúdo se adapta sem cortes, sobreposições ou rolagem horizontal e o texto permanece legível

#### Scenario: Movimento reduzido
- **WHEN** o sistema operacional indica `prefers-reduced-motion`
- **THEN** animações decorativas de cards, selos e botões são desativadas ou reduzidas sem afetar o uso das páginas

#### Scenario: Estados dinâmicos anunciados
- **WHEN** a contagem de favoritos, uma mensagem de erro ou o estado de carregamento muda
- **THEN** a mudança é exposta em elementos com `role="status"` ou `role="alert"`, preservando os atributos `aria-live`, `aria-pressed`, `aria-label` e `role` existentes

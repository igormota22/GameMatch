# identidade-visual Specification

## Purpose

Define a identidade visual da locadora de videogames dos anos 90 aplicada ao design system global do GameMatch, ao shell do site (cabeçalho e rodapé) e à página inicial, garantindo que a reformulação seja reconhecível, acessível e sem impacto na funcionalidade existente (RF-19 do `docs/prd.md`).

## Requirements

### Requirement: Design system da locadora como base visual
O sistema SHALL dispor de um design system global — tokens de cor (azul-marinho profundo, azul elétrico, amarelo vibrante, laranja/vermelho promocional, branco envelhecido/papel, cinza e preto), tipografia inspirada em revistas e embalagens, texturas sutis de papel/plástico, bordas e sombras de objeto físico — aplicado como fonte única de estilo da interface (RF-19; DT-04).

#### Scenario: Páginas utilizam os tokens da identidade
- **WHEN** qualquer página do site é renderizada
- **THEN** cores, tipografia e superfícies observadas pertencem aos tokens da identidade da locadora, sem o uso da paleta neon roxa/azul de interfaces gaming modernas

#### Scenario: Texturas e materiais percebidos
- **WHEN** o usuário visualiza superfícies como fundos, painéis e cards
- **THEN** há percepção de materiais físicos (papel/plástico impresso) por meio de texturas sutis, bordas e sombras, sem prejuízo da legibilidade do texto

### Requirement: Shell do site com identidade da locadora
O cabeçalho e o rodapé SHALL apresentar a identidade da locadora (marca, paleta, tipografia e elementos gráficos) em todas as rotas, para visitantes e usuários autenticados (RF-19; RNF-03).

#### Scenario: Visitante acessa qualquer rota
- **WHEN** um visitante abre qualquer página pública
- **THEN** o cabeçalho e o rodapé exibem a identidade visual da locadora e a marca GameMatch permanece visível e clicável para `/`

#### Scenario: Usuário autenticado navega
- **WHEN** um usuário autenticado acessa o site
- **THEN** o cabeçalho mantém os rótulos, rotas e estados de navegação atuais (Início, Catálogo, Recomendações, Favoritos, Perfil e Sair), sem um atalho próprio para `Minha conta`

### Requirement: Preservar navegação e atribuição FreeToGame
A reformulação do shell SHALL preservar os rótulos e destinos de navegação existentes, com a única mudança de o acesso a `/conta` passar a ser oferecido na página de perfil, e SHALL manter no rodapé o link ativo de atribuição à FreeToGame.com em todas as páginas (RF-19; RNF-03; CA-19).

#### Scenario: Navegação sem mudança de comportamento
- **WHEN** o usuário usa o menu do cabeçalho em qualquer estado de sessão
- **THEN** cada item leva à mesma rota de antes da reformulação, com o mesmo texto visível, exceto pela ausência do item `Minha conta`

#### Scenario: Rodapé com atribuição
- **WHEN** qualquer página (pública ou autenticada) é exibida
- **THEN** o rodapé exibe a atribuição à FreeToGame.com com link ativo e clicável

### Requirement: Página inicial como fachada da locadora
A página inicial SHALL apresentar composição de fachada/vitrine da locadora — identidade marcante, chamadas promocionais e destaques gráficos — mantendo o conteúdo funcional atual: bloco de status da API e, para autenticados, resumo do perfil com os CTAs existentes (RF-19; RNF-05; RF-05).

#### Scenario: Visitante abre a página inicial
- **WHEN** um visitante acessa `/`
- **THEN** a página transmite a sensação de entrar em uma locadora (fachada/vitrine) e o bloco de status da API continua visível e funcional

#### Scenario: Autenticado abre a página inicial
- **WHEN** um usuário autenticado acessa `/`
- **THEN** o resumo do perfil, os avisos de perfil incompleto e os CTAs (perfil, recomendações, favoritos) permanecem presentes e operacionais dentro da nova composição

### Requirement: Elementos gráficos de locadora reutilizáveis
O sistema SHALL oferecer elementos gráficos reutilizáveis — etiquetas, selos/adesivos promocionais, carimbos e fichas de locação — utilizáveis em qualquer página e aplicados na página inicial (RF-19).

#### Scenario: Etiquetas promocionais na página inicial
- **WHEN** a página inicial é renderizada
- **THEN** há elementos gráficos de locadora (etiquetas/selos) reforçando a identidade, sem substituir nem esconder o conteúdo funcional

### Requirement: Interações físicas sem prejudicar a navegação
As interações SHALL lembrar a metáfora física da locadora (hover de retirar caixa da prateleira, etiquetas ao passar o mouse, aberturas de fichas/embalagens) e SHALL respeitar `prefers-reduced-motion`, não bloquear o uso por teclado nem esconder conteúdo essencial apenas em hover (RF-19; RNF-07).

#### Scenario: Navegação por teclado
- **WHEN** o usuário navega por teclado até um elemento interativo
- **THEN** o foco é visível e o conteúdo e as ações permanecem acessíveis sem depender do mouse

#### Scenario: Usuário prefere movimento reduzido
- **WHEN** o sistema operacional do usuário indica `prefers-reduced-motion`
- **THEN** animações decorativas são desativadas ou reduzidas sem afetar a funcionalidade

### Requirement: Funcionalidade e responsividade preservadas
A reformulação SHALL alterar apenas a apresentação: rotas, serviços, regras de negócio, textos exigidos por critérios de aceite e testes automatizados permanecem válidos, e a interface permanece responsiva nos breakpoints existentes (RF-19; RNF-07; RNF-08; CA-19/CA-20).

#### Scenario: Suíte de testes permanece verde
- **WHEN** a suíte de testes do frontend é executada após a reformulação
- **THEN** todos os testes existentes continuam passando (sem alteração de comportamento testado)

#### Scenario: Responsividade mantida
- **WHEN** a página inicial e o shell são exibidos em viewport móvel e desktop
- **THEN** o layout se adapta sem cortes, sobreposições ou rolagem horizontal indevida

### Requirement: Página de login com a identidade da locadora
A página `/entrar` SHALL exibir a identidade da locadora preservando os campos de e-mail e senha, as validações, o botão de entrada, o link para criar conta e o redirecionamento seguro após o login (RF-19; RF-02; CA-03).

#### Scenario: Visitante abre a página de login
- **WHEN** um visitante acessa `/entrar`
- **THEN** a página exibe a identidade da locadora e os textos `Entrar na sua conta`, `E-mail`, `Senha`, `Entrar`, `Ainda não tem conta?` e `Criar conta`, com os campos e o botão operacionais

#### Scenario: Credenciais válidas iniciam a sessão
- **WHEN** o visitante informa credenciais válidas e envia o formulário
- **THEN** a sessão é iniciada e ele é redirecionado para o `destino` seguro informado na URL ou para `/`

#### Scenario: Credenciais inválidas mantêm o usuário no login
- **WHEN** a API rejeita as credenciais
- **THEN** a mensagem de erro é exibida na página, nenhuma sessão é criada e o formulário continua disponível para nova tentativa

#### Scenario: Validação local impede envio inválido
- **WHEN** o visitante tenta enviar e-mail inválido ou senha vazia
- **THEN** as mensagens `Informe um e-mail válido.` e `A senha é obrigatória.` aparecem e nenhuma requisição é enviada

### Requirement: Página de cadastro com a identidade da locadora
A página `/criar-conta` SHALL exibir a identidade da locadora preservando os campos de e-mail e senha, a dica de senha, as validações, o botão de cadastro, o link para entrar e a autenticação automática após a criação (RF-19; RF-01; CA-01; CA-02).

#### Scenario: Visitante abre a página de cadastro
- **WHEN** um visitante acessa `/criar-conta`
- **THEN** a página exibe a identidade da locadora e os textos `Criar conta`, `E-mail`, `Senha`, `Mínimo de 8 caracteres, com ao menos uma maiúscula, uma minúscula e um número.`, `Já tem conta?` e `Entrar`, com os campos e o botão operacionais

#### Scenario: Cadastro válido autentica automaticamente
- **WHEN** o visitante informa um e-mail novo e uma senha válida
- **THEN** a conta é criada, a sessão é iniciada e ele é redirecionado para `/conta`

#### Scenario: E-mail já cadastrado é recusado
- **WHEN** a API informa conflito para o e-mail informado
- **THEN** a mensagem de conflito é exibida, nenhuma conta duplicada é criada e o formulário continua disponível

#### Scenario: Validação local impede envio inválido
- **WHEN** o visitante tenta enviar e-mail inválido ou senha com menos de 8 caracteres
- **THEN** as mensagens `Informe um e-mail válido.` e `A senha precisa ter pelo menos 8 caracteres.` aparecem e nenhuma requisição é enviada

### Requirement: Página Minha conta com a identidade da locadora
A página `/conta` SHALL exibir a identidade da locadora preservando os dados da conta, o botão `Sair`, o formulário de alteração de senha, as mensagens de carregamento, erro e sucesso e a proteção de rota (RF-19; RF-03; RF-04; CA-04; CA-05).

#### Scenario: Usuário autenticado abre a conta
- **WHEN** um usuário autenticado acessa `/conta`
- **THEN** a página exibe a identidade da locadora e os textos `Minha conta`, `Seus dados de acesso ao GameMatch.`, `Dados da conta`, `E-mail`, `Conta criada em`, `Alterar senha`, `Senha atual`, `Nova senha` e `Sair`, com o e-mail e a data de criação da conta

#### Scenario: Alteração de senha bem-sucedida
- **WHEN** o usuário informa a senha atual correta e uma nova senha válida
- **THEN** a mensagem `Senha alterada com sucesso.` é exibida e o formulário é limpo

#### Scenario: Alteração de senha recusada
- **WHEN** a API rejeita a alteração por senha atual incorreta ou nova senha inválida
- **THEN** a mensagem de erro é exibida e o formulário permanece disponível para correção

#### Scenario: Rota protegida sem sessão
- **WHEN** um visitante sem sessão acessa `/conta`
- **THEN** ele é redirecionado para `/entrar?destino=/conta`

### Requirement: Formulários acessíveis e responsivos
Os formulários das páginas de autenticação e de conta SHALL manter rótulos associados aos campos, erros identificáveis por tecnologia assistiva, foco visível, contraste AA, layout responsivo sem rolagem horizontal e interações que respeitam `prefers-reduced-motion` (RF-19; RNF-07; RNF-08; CA-23).

#### Scenario: Navegação por teclado nos formulários
- **WHEN** o usuário percorre o formulário com Tab
- **THEN** cada campo e botão recebe foco visível, na ordem do formulário, e os erros permanecem identificáveis por leitores de tela

#### Scenario: Layout em viewport móvel e desktop
- **WHEN** as três páginas são exibidas em 390px e 1440px
- **THEN** o conteúdo se adapta sem cortes, sobreposições ou rolagem horizontal e o texto permanece legível

#### Scenario: Movimento reduzido
- **WHEN** o sistema operacional indica `prefers-reduced-motion`
- **THEN** animações decorativas das três páginas são desativadas ou reduzidas sem afetar o uso dos formulários

### Requirement: Acesso à conta a partir do perfil
A página `/perfil` SHALL oferecer uma ação visível e acessível que leve o usuário autenticado a `/conta`, substituindo o atalho próprio `Minha conta` que existia no cabeçalho (RF-19; RF-03; J-8).

#### Scenario: Usuário acessa a conta pelo perfil
- **WHEN** um usuário autenticado abre `/perfil`
- **THEN** a página exibe a ação `Minha conta` e, ao acioná-la, o usuário é levado a `/conta`

#### Scenario: Cabeçalho sem atalho de conta
- **WHEN** um usuário autenticado navega pelo cabeçalho
- **THEN** o item `Minha conta` não é exibido e os demais itens permanecem inalterados

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

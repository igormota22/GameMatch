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

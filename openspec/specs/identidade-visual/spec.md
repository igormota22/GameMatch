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
- **THEN** o cabeçalho mantém os mesmos rótulos, rotas e estados de navegação atuais (Início, Catálogo, Recomendações, Favoritos, Perfil, Minha conta, Sair)

### Requirement: Preservar navegação e atribuição FreeToGame
A reformulação do shell SHALL preservar integralmente os rótulos e destinos de navegação existentes e manter no rodapé o link ativo de atribuição à FreeToGame.com em todas as páginas (RF-19; RNF-03; CA-19).

#### Scenario: Navegação sem mudança de comportamento
- **WHEN** o usuário usa o menu do cabeçalho em qualquer estado de sessão
- **THEN** cada item leva à mesma rota de antes da reformulação, com o mesmo texto visível

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

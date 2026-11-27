# Spec Delta

## ADDED Requirements

### Requirement: Página 404 com a identidade da locadora
A página de rota inexistente SHALL exibir a identidade da locadora preservando os textos `Erro 404`, `Página não encontrada`, `O endereço acessado não existe ou foi movido.` e o link `Voltar para o início` para `/`, além do título `GameMatch — Página não encontrada` e do shell com atribuição à FreeToGame (RF-19; CA-19; CA-23).

#### Scenario: Visitante acessa um endereço inexistente
- **WHEN** um visitante abre qualquer rota não cadastrada
- **THEN** a página exibe a identidade da locadora, o `h1` `Página não encontrada`, o texto `Erro 404`, a orientação `O endereço acessado não existe ou foi movido.` e o link `Voltar para o início` apontando para `/`

#### Scenario: Rota e título preservados
- **WHEN** a rota wildcard é carregada
- **THEN** o título do documento permanece `GameMatch — Página não encontrada`, o componente continua com carregamento lazy e a rota permanece pública, sem guard

#### Scenario: Acessibilidade e responsividade
- **WHEN** a página é exibida em 390px e 1440px ou percorrida apenas com Tab
- **THEN** o layout se adapta sem rolagem horizontal, o link recebe foco visível na ordem do conteúdo e o contraste atende AA

#### Scenario: Movimento reduzido
- **WHEN** o sistema operacional indica `prefers-reduced-motion`
- **THEN** animações decorativas da página são desativadas ou reduzidas sem afetar a leitura nem o link de retorno

#### Scenario: Shell e atribuição mantidos
- **WHEN** a página 404 é exibida
- **THEN** o cabeçalho e o rodapé da locadora permanecem visíveis, incluindo a atribuição à FreeToGame.com com link ativo

### Requirement: Vitrine de favoritos na página inicial
A vitrine da página inicial SHALL exibir até três jogos favoritos do usuário autenticado, com capa, título e acesso a `/favoritos`, e SHALL apresentar mensagens adequadas para visitante, lista vazia, carregamento e falha, sem comprometer o status da API nem o resumo do perfil (RF-19; RF-16; CA-16; CA-23).

#### Scenario: Usuário autenticado com favoritos
- **WHEN** um usuário autenticado com favoritos abre `/`
- **THEN** a vitrine exibe no máximo três jogos favoritos, cada um com capa, texto alternativo e título, substituindo as capas genéricas, e oferece acesso a `/favoritos`

#### Scenario: Usuário autenticado sem favoritos
- **WHEN** um usuário autenticado sem favoritos abre `/`
- **THEN** a vitrine exibe `Você ainda não tem jogos favoritos.` e o link `Explorar catálogo` para `/catalogo`

#### Scenario: Visitante sem conta
- **WHEN** um visitante abre `/`
- **THEN** a vitrine exibe `Entre para ver seus favoritos.` e o link `Entrar` para `/entrar`, mantendo visível o bloco de status da API

#### Scenario: Falha ao carregar os favoritos
- **WHEN** a leitura dos favoritos falha na página inicial
- **THEN** a vitrine exibe uma mensagem de aviso em português com link para `/favoritos`, e o status da API e o resumo do perfil continuam disponíveis

#### Scenario: Carregamento dos favoritos
- **WHEN** a leitura dos favoritos está em andamento
- **THEN** a vitrine anuncia `Carregando favoritos...` em um elemento com `role="status"` e não esconde o restante da página

#### Scenario: Acessibilidade da vitrine
- **WHEN** a vitrine é exibida em 390px e 1440px ou percorrida apenas com Tab
- **THEN** as capas têm texto alternativo, os links recebem foco visível e o layout se adapta sem rolagem horizontal

## MODIFIED Requirements

### Requirement: Página de cadastro com a identidade da locadora
A página `/criar-conta` SHALL exibir a identidade da locadora preservando os campos de e-mail e senha, a dica de senha, as validações, o botão de cadastro, o link para entrar, a autenticação automática e o redirecionamento para `/perfil` após a criação (RF-19; RF-01; CA-01; CA-02).

#### Scenario: Visitante abre a página de cadastro
- **WHEN** um visitante acessa `/criar-conta`
- **THEN** a página exibe a identidade da locadora e os textos `Criar conta`, `E-mail`, `Senha`, `Mínimo de 8 caracteres, com ao menos uma maiúscula, uma minúscula e um número.`, `Já tem conta?` e `Entrar`, com os campos e o botão operacionais

#### Scenario: Cadastro válido autentica automaticamente
- **WHEN** o visitante informa um e-mail novo e uma senha válida
- **THEN** a conta é criada, a sessão é iniciada e ele é redirecionado para `/perfil`

#### Scenario: E-mail já cadastrado é recusado
- **WHEN** a API informa conflito para o e-mail informado
- **THEN** a mensagem de conflito é exibida, nenhuma conta duplicada é criada e o formulário continua disponível

#### Scenario: Validação local impede envio inválido
- **WHEN** o visitante tenta enviar e-mail inválido ou senha com menos de 8 caracteres
- **THEN** as mensagens `Informe um e-mail válido.` e `A senha precisa ter pelo menos 8 caracteres.` aparecem e nenhuma requisição é enviada

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

# PRD - Fundação do projeto

## Identificação

| Campo | Valor |
|---|---|
| Change | `fundacao-projeto` |
| Status do PRD | Aprovado |
| Revisor | Desenvolvedor responsável |
| Revisado em | 2026-11-10 |

> Estado atual: 26 de 26 tarefas implementadas e verificadas. Este PRD consolida
> o planejamento e o estado real da change, incluindo as evidências registradas
> em `docs/validacao-fundacao.md`.

## Problema e objetivo

O projeto partiu de um repositório greenfield: existiam apenas `README.md`,
`AGENTE.md` e o scaffolding do OpenSpec. Sem uma fundação reproduzível, as fases
seguintes (identidade, catálogo e recomendações) não teriam onde rodar e a
demonstração do TCC não poderia ser executada em ambiente limpo.

Esta change já entregou o monorepo, a solution .NET em camadas e o backend base.
Falta concluir o shell web e executar a verificação integrada da fundação.

**Objetivo:** entregar um ambiente executável e observável do GameMatch —
monorepo, API em camadas, PostgreSQL local, shell Angular, testes automatizados e
documentação operacional — para que as próximas fases tenham uma base
reproduzível e verificável.

## Escopo

### O que muda

- Inicializa o repositório git e o monorepo com `api/`, `web/`, `seed/` e `docs/`.
- Cria a solution .NET 10 em quatro camadas (`Domain`, `Application`,
  `Infrastructure`, `Api`) mais dois projetos de teste (`UnitTests`,
  `IntegrationTests`).
- Cria o app Angular 22.2.0 zoneless, com componentes standalone, signals,
  Tailwind CSS e Lucide icons.
- Sobe PostgreSQL via Docker Compose e configura o EF Core com a migration
  inicial.
- Adiciona health check, Swagger/OpenAPI, ProblemDetails, CORS, logging e
  configuração por ambiente.
- Adiciona o harness de testes: xUnit no backend e o runner padrão do Angular CLI
  no frontend.
- Adiciona o rodapé de atribuição à FreeToGame com link ativo, conforme os
  termos da API.
- Documenta pré-requisitos, comandos de execução e arquitetura em `README.md` e
  `docs/`.

### Fora de escopo

- Autenticação, catálogo, sincronização FreeToGame, recomendações, favoritos e
  avaliações.
- Deploy em nuvem, CI/CD, domínio público e observabilidade distribuída.
- Design system completo, temas, dark mode e i18n.
- Integração com a API FreeToGame nesta fase; apenas o link de atribuição.

### Capacidades afetadas

- `plataforma-base` (nova): ambiente executável e observável do GameMatch,
  cobrindo repositório, API, banco, shell Angular, testes, documentação
  operacional e atribuição da fonte de dados. Os requisitos dessa capacidade
  estão na seção **Requisitos** deste PRD.

### Impacto

- **Novo código:** `api/`, `web/`, `docker-compose.yml`, `seed/`, `docs/`.
- **Ferramentas:** .NET 10.0.103, Node 24.21, Angular CLI 22.2.4, Docker 29.7.
- **Banco:** PostgreSQL em container local; porta configurável por variável de
  ambiente.
- **Externo:** atribuição à FreeToGame exigida pelos termos de uso; nenhuma
  chamada à API nesta fase.
- **Sem mudanças disruptivas:** não há código ou comportamento anterior a
  preservar.
- **Riscos operacionais:** builds dentro do OneDrive podem sofrer locks;
  Tailwind 4 + Angular 22 exige spike de configuração; conflito de porta do
  PostgreSQL local.

## Requisitos

### Requirement: Ambiente local reproduzível

O sistema DEVE (MUST) ser iniciável a partir de um checkout limpo com comandos
documentados para banco, API e aplicação web, sem edição manual de código.

#### Scenario: Subida completa a partir do zero

- **WHEN** um desenvolvedor segue os comandos documentados em uma máquina com os
  pré-requisitos instalados
- **THEN** o banco está acessível, a API responde ao health check e a aplicação
  web carrega no navegador

#### Scenario: Pré-requisito ausente

- **WHEN** um pré-requisito obrigatório não está instalado
- **THEN** a documentação identifica o pré-requisito e o comando de correção
  antes da execução

### Requirement: Saúde e conectividade com o banco

O sistema DEVE (MUST) expor um endpoint de saúde que informe o estado da API e da
conexão com o banco relacional.

#### Scenario: Banco disponível

- **WHEN** o endpoint de saúde é consultado com o banco disponível
- **THEN** a resposta indica estado saudável e conexão com o banco bem-sucedida

#### Scenario: Banco indisponível

- **WHEN** o endpoint de saúde é consultado com o banco indisponível
- **THEN** a resposta indica estado não saudável e identifica a falha de conexão

### Requirement: Documentação do contrato HTTP

O sistema DEVE (MUST) disponibilizar documentação OpenAPI navegável do contrato
HTTP em ambiente de desenvolvimento.

#### Scenario: Consulta à documentação

- **WHEN** a documentação da API é aberta no ambiente de desenvolvimento
- **THEN** os endpoints e schemas disponíveis são listados e podem ser
  inspecionados

### Requirement: Shell web responsivo em português

O shell web DEVE (MUST) carregar em uma rota pública, exibir navegação e rodapé
em português e permanecer utilizável em telas de 360 px a 1440 px.

#### Scenario: Tela pequena

- **WHEN** a rota pública é aberta em uma viewport de 360 px de largura
- **THEN** não há rolagem horizontal e o conteúdo principal permanece legível e
  operável

#### Scenario: Tela ampla

- **WHEN** a rota pública é aberta em uma viewport de 1440 px de largura
- **THEN** o layout ocupa o espaço disponível sem quebra de estrutura

### Requirement: Atribuição à FreeToGame

O shell web DEVE (MUST) exibir a atribuição à FreeToGame.com em seu rodapé, com
link ativo para https://www.freetogame.com, aberto em nova aba com
`rel="noopener"`.

#### Scenario: Atribuição visível

- **WHEN** qualquer página do shell web é renderizada
- **THEN** o rodapé contém o texto de atribuição e um link ativo para a FreeToGame

#### Scenario: Abertura do link

- **WHEN** o usuário aciona o link de atribuição
- **THEN** o site da FreeToGame abre em nova aba e a página de origem não expõe
  `window.opener`

### Requirement: Configuração sem segredos no repositório

O sistema DEVE (MUST) ler strings de conexão e segredos de variáveis de ambiente
ou de user-secrets, e o repositório NÃO DEVE (MUST NOT) conter segredos
versionados.

#### Scenario: Inicialização sem configuração obrigatória

- **WHEN** a API inicia sem a string de conexão obrigatória
- **THEN** a inicialização falha com mensagem clara que identifica a variável de
  ambiente ausente

#### Scenario: Varredura do repositório

- **WHEN** o conteúdo versionado é inspecionado
- **THEN** não há chaves, senhas ou strings de conexão reais, e os arquivos
  locais de segredo estão ignorados

### Requirement: Erros padronizados na API

As respostas de erro da API DEVEM (MUST) seguir o formato ProblemDetails
(RFC 7807), incluindo status, título, detalhe e identificador de correlação.

#### Scenario: Requisição inválida

- **WHEN** uma requisição contém dados inválidos
- **THEN** a API responde com status 400, content-type
  `application/problem+json` e os campos padronizados

#### Scenario: Falha inesperada

- **WHEN** ocorre uma exceção não tratada durante uma requisição
- **THEN** a API responde com status 500 e ProblemDetails contendo o identificador
  de correlação, sem stack trace em produção

### Requirement: Testes automatizados executáveis

O projeto DEVE (MUST) oferecer comandos únicos para executar a suíte de testes do
backend e a suíte de testes do frontend, e ambas DEVEM (MUST) passar no estado
inicial.

#### Scenario: Testes do backend

- **WHEN** o comando de testes do backend é executado
- **THEN** a suíte é descoberta, executada e concluída com sucesso

#### Scenario: Testes do frontend

- **WHEN** o comando de testes do frontend é executado
- **THEN** a suíte é descoberta, executada e concluída com sucesso

## Decisões técnicas

### 1. Monorepo com `api/`, `web/`, `seed/` e `docs/`

Um único repositório simplifica a demo, o versionamento dos artefatos OpenSpec e
a revisão do TCC. Repositórios separados adicionariam coordenação sem benefício
para um projeto individual.

### 2. Backend em quatro camadas

```
GameMatch.Api  -->  GameMatch.Infrastructure  -->  GameMatch.Application  -->  GameMatch.Domain
```

- `Domain` não depende de nada: entidades, objetos de valor e, na fase 4, a
  calculadora de compatibilidade.
- `Application` depende de `Domain`: casos de uso, DTOs, validação e interfaces
  de repositório.
- `Infrastructure` depende de `Application`/`Domain`: EF Core, Npgsql e, na fase
  2, o cliente FreeToGame.
- `Api` depende de `Application`/`Infrastructure`: controllers, autenticação,
  Swagger e composição.

Alternativas consideradas: vertical slices (mais rápidas, porém menos alinhadas
ao requisito de "arquitetura em camadas" do README) e projeto único (simples, mas
indefensável na banca).

### 3. .NET 10 com controllers

Controllers explícitos tornam o contrato HTTP e o Swagger mais legíveis para a
banca do que minimal APIs, além de facilitarem a organização por recurso nas
fases seguintes. Minimal APIs ficam como alternativa se o volume de endpoints
crescer.

### 4. PostgreSQL em Docker, EF Core e uma entidade de infraestrutura

O README pede banco relacional; PostgreSQL é robusto e o Docker já está
instalado. SQLite seria mais simples, mas se afasta do cenário de produção e do
requisito de banco relacional.

Para comprovar o pipeline de migrations sem antecipar o catálogo, a fundação cria
a entidade de infraestrutura `RegistroSistema` (`Chave`, `Valor`,
`AtualizadoEm`), usada para versão de schema e estado de seed. Alternativa:
migration vazia; ela prova o ferramental, mas não exercita uma tabela real.

### 5. Angular 22.2.0 zoneless, standalone e signals

Decisão imposta pelo `AGENTE.md`. O bootstrap usa `bootstrapApplication` e
`provideZonelessChangeDetection`; rotas são declaradas com `provideRouter`; o
estado começa com signals simples, sem NgRx. NgModules e zone.js ficam fora.

### 6. Tailwind CSS 4 com spike e fallback

A integração usa o caminho oficial do Tailwind 4 via PostCSS
(`@tailwindcss/postcss`) e `@import "tailwindcss";` no estilo global. Como
Angular 22 + Tailwind 4 é uma combinação recente, a primeira tarefa é um spike de
build. Se o pipeline rejeitar, o fallback documentado é Tailwind 3 com
`tailwind.config.js`, sem mudar o contrato visual.

### 7. Lucide icons com importação seletiva

Uso de `lucide-angular` com ícones importados individualmente (`Gamepad2`,
`ExternalLink`, `Menu`), evitando pacote inteiro no bundle. Alternativa: SVGs
inline; mais controle, porém mais manutenção.

### 8. Estrutura do frontend por camadas

```
web/src/app/
+-- core/        -> configuração, HTTP, guards e serviços globais
+-- shared/      -> componentes reutilizáveis (botão, card, estado vazio)
+-- layout/      -> shell, header, footer
+-- features/    -> uma pasta por funcionalidade nas fases seguintes
```

Rotas lazy por feature a partir da fase 1. A fundação entrega apenas a rota
pública `/` com shell e uma página placeholder, além da rota 404.

### 9. Erros padronizados com ProblemDetails

A API usa ProblemDetails (RFC 7807) com `traceId` derivado de
`HttpContext.TraceIdentifier`. Um middleware global captura exceções não
tratadas; em produção, o detalhe não expõe stack trace. Alternativa: envelope de
erro próprio; rejeitado por não ser padrão e por dificultar ferramentas.

### 10. Health check com verificação do PostgreSQL

`/health` é anônimo e retorna JSON com o estado geral e o check do banco
(Npgsql). Não há separação liveness/readiness nesta fase; a demo local não
precisa. Alternativa: endpoint customizado, sem ganho.

### 11. OpenAPI com Swagger UI em Development

O documento OpenAPI é gerado pelo suporte nativo do ASP.NET Core e exposto via
Swagger UI apenas em desenvolvimento. Alternativa: Scalar; pode substituir a UI
sem alterar o contrato.

### 12. CORS restrito e proxy de desenvolvimento

A API aceita apenas as origens configuradas em `Cors:AllowedOrigins`. O Angular
usa `proxy.conf.json` para encaminhar `/api` ao backend, reduzindo atrito de CORS
no desenvolvimento. CORS permissivo em dev foi rejeitado por esconder problemas
que apareceriam depois.

### 13. Configuração e segredos

A string de conexão vem de `ConnectionStrings:GameMatch` via variável de ambiente
ou user-secrets. O `docker-compose.yml` usa variáveis de `.env` (ignorado) e
`.env.example` versionado com placeholders. O `.gitignore` cobre `bin/`, `obj/`,
`.env`, `*.user`, `.vs/` e arquivos de segredo.

### 14. Testes

- Backend: xUnit; um teste de unidade de fumaça e um teste de integração com
  `WebApplicationFactory`. Testcontainers é o caminho preferido para PostgreSQL;
  se o Docker não estiver disponível no ambiente de teste, o fallback é um banco
  de teste dedicado com nome próprio.
- Frontend: runner padrão do Angular CLI 22 (Vitest ou Karma, conforme o
  scaffold). Um teste de fumaça do shell é suficiente nesta fase.

### 15. Nomes e idioma

Textos de interface em PT-BR. Identificadores de domínio, métodos e variáveis em
português, seguindo as convenções da linguagem
(`CalculadoraCompatibilidade`, `Jogo`, `cartaoJogo`). Nomes de frameworks,
pacotes e configurações permanecem em inglês.

### 16. Atribuição à FreeToGame

Um componente de rodapé renderiza "Dados fornecidos por FreeToGame.com" com link
para `https://www.freetogame.com`, `target="_blank"` e
`rel="noopener noreferrer"`. A URL fica em configuração para evitar duplicação. É
requisito dos termos de uso da API.

### 17. Runbook local

Fluxo documentado:

```
docker compose up -d db
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
dotnet run --project api/src/GameMatch.Api
cd web && npm install && npm start
```

Portas padrão: API `5080`, web `4200`, PostgreSQL `5432` (sobrescrevível por
`.env`). O `README.md` registra pré-requisitos e solução de conflitos.

## Riscos e trade-offs

- **OneDrive e locks de build** → mover o projeto para `C:\dev\GameMatch` ou
  pausar a sincronização durante os builds; documentar no runbook.
- **Tailwind 4 + Angular 22** → spike no primeiro dia; fallback para Tailwind 3
  sem alterar o contrato visual.
- **Migration com entidade de infraestrutura** → adiciona uma tabela pequena e
  artificial; será reutilizada para metadados de schema/seed e não vaza para o
  domínio.
- **Testcontainers depende de Docker** → fallback para banco de teste dedicado;
  testes de integração ficam condicionais ao Docker disponível.
- **Runner de testes do Angular 22** → usar o padrão do CLI; se instável, fixar
  Karma/Jasmine sem mudar o requisito de suíte passando.
- **Pacotes .NET 10 recentes** → fixar versões no `Directory.Packages.props` ou
  nos `.csproj` e validar no primeiro build.
- **Swagger UI em .NET 10** → se o pacote não suportar a versão, trocar a UI por
  Scalar mantendo o documento OpenAPI.
- **Sem CI nesta fase** → regressões dependem de execução local; aceitável para o
  prazo, com possibilidade de adicionar GitHub Actions depois.
- **Atribuição externa** → o teste verifica `href`, `target` e `rel`, não a
  disponibilidade do site.

## Plano de execução e rollback

Não há dados ou comportamento anterior. A mudança é aditiva:

1. Inicializar git e criar a estrutura de diretórios.
2. Subir o PostgreSQL e aplicar a migration inicial.
3. Executar API e web, validar health, Swagger e shell.
4. Rodar as suítes de teste.
5. Rollback: remover os diretórios criados e os volumes locais do Docker; nenhum
   ambiente de produção é afetado.

## Tarefas

## 1. Repositório e estrutura

- [x] 1.1 Inicializar o repositório git e ampliar o `.gitignore` para `bin/`,
  `obj/`, `.env`, `*.user`, `.vs/` e segredos; verificar com `git status` que
  esses caminhos não aparecem como rastreáveis.
- [x] 1.2 Criar a estrutura `api/`, `web/`, `seed/` e `docs/`, com
  `seed/README.md` explicando o snapshot futuro do catálogo; verificar que os
  diretórios e o arquivo existem.
- [x] 1.3 Criar `api/GameMatch.sln` com os projetos `GameMatch.Domain`,
  `GameMatch.Application`, `GameMatch.Infrastructure`, `GameMatch.Api`,
  `GameMatch.UnitTests` e `GameMatch.IntegrationTests`; verificar que
  `dotnet build api/GameMatch.sln` conclui sem erros.
- [x] 1.4 Adicionar `Directory.Build.props` com nullable e implicit usings
  habilitados e versões de pacotes fixadas; verificar build limpo com
  `dotnet build api/GameMatch.sln`.
- [x] 1.5 Criar `docs/arquitetura.md` com o diagrama de camadas e as decisões da
  fundação, e um esqueleto de quickstart no `README.md`; verificar que os links e
  comandos citados existem.

## 2. Backend base

- [x] 2.1 Configurar a API em ASP.NET Core com controllers, porta `5080`,
  `appsettings` por ambiente e leitura de `ConnectionStrings:GameMatch` por
  variável de ambiente ou user-secrets; verificar que `dotnet run` inicia sem
  segredo versionado.
- [x] 2.2 Adicionar o manifest local do `dotnet-ef`, configurar EF Core + Npgsql,
  criar o `GameMatchDbContext` e a entidade de infraestrutura `RegistroSistema`;
  verificar `dotnet tool restore`, `dotnet ef migrations add Inicial` e
  `dotnet ef database update` criando a tabela.
- [x] 2.3 Criar `docker-compose.yml` com PostgreSQL e `.env.example` com
  placeholders; verificar `docker compose up -d db` e `docker compose ps` com o
  serviço saudável.
- [x] 2.4 Implementar `/health` com verificação da conexão PostgreSQL; verificar
  resposta 200 com o banco disponível e resposta não saudável com o banco parado.
- [x] 2.5 Configurar ProblemDetails, `UseExceptionHandler` e
  `UseStatusCodePages`; adicionar teste de integração com endpoint de teste que
  lança exceção e verificar 500 com `application/problem+json` e `traceId`, sem
  stack trace em produção.
- [x] 2.6 Configurar CORS por origem permitida e logging com identificador de
  correlação; verificar cabeçalho `Access-Control-Allow-Origin` para a origem
  permitida e ausência para origem não permitida.
- [x] 2.7 Configurar documento OpenAPI e Swagger UI em Development; verificar que
  `/swagger` carrega e lista o endpoint `/health`.
- [x] 2.8 Adicionar xUnit, um teste de fumaça e um teste de integração com
  `WebApplicationFactory` (Testcontainers quando o Docker estiver disponível;
  banco de teste dedicado como fallback); verificar `dotnet test` verde.
- [x] 2.9 Documentar no `README.md` e em `docs/ambiente.md` o setup da API, as
  variáveis de ambiente, as portas e os comandos EF; verificar que os comandos
  documentados executam como escritos.

## 3. Shell web

- [x] 3.1 Criar o app Angular 22.2.0 com `bootstrapApplication`,
  `provideZonelessChangeDetection`, componentes standalone e signals; verificar
  `npm start` e que `zone.js` não é importado.
- [x] 3.2 Executar o spike de Tailwind 4 via PostCSS, com fallback documentado
  para Tailwind 3 se o build do Angular 22 rejeitar a integração; verificar uma
  classe utilitária aplicada na página inicial.
- [x] 3.3 Implementar o shell com header, conteúdo principal, rodapé, rota
  pública `/`, placeholder e rota 404, com textos em PT-BR; verificar navegação e
  renderização das rotas.
- [x] 3.4 Ajustar a responsividade entre 360 px e 1440 px; verificar ausência de
  rolagem horizontal e legibilidade em 360, 768 e 1440 px.
- [x] 3.5 Adicionar a atribuição "Dados fornecidos por FreeToGame.com" no rodapé
  com link ativo, `target="_blank"` e `rel="noopener noreferrer"`; verificar
  texto, `href`, `target` e `rel`.
- [x] 3.6 Configurar `proxy.conf.json` para encaminhar `/api` à API e habilitar
  `provideHttpClient`; verificar uma chamada do frontend que alcança o `/health`
  da API pelo proxy.
- [x] 3.7 Adicionar um teste de fumaça do shell usando o runner padrão do Angular
  CLI 22; verificar `npm test -- --watch=false` verde.
- [x] 3.8 Documentar no `README.md` e em `docs/ambiente.md` o setup do frontend,
  as portas e o proxy; verificar que os comandos documentados executam como
  escritos.

## 4. Verificação integrada da fundação

- [x] 4.1 Executar o runbook em um checkout limpo (sem caches): subir o banco,
  aplicar migrations, iniciar API e web; verificar `/health`, `/swagger` e o shell
  no navegador.
- [x] 4.2 Executar `dotnet test api/GameMatch.sln` e `npm test -- --watch=false`
  em `web/`; verificar ambas as suítes verdes.
- [x] 4.3 Verificar que nenhum segredo está versionado, inspecionando `.env`,
  `.env.example`, `appsettings*.json` e uma busca por senhas/chaves; verificar que
  apenas placeholders estão no repositório.
- [x] 4.4 Percorrer os cenários da seção **Requisitos** (health com banco up/down,
  CORS permitido/negado, ProblemDetails 400/500, atribuição, responsividade
  360/1440) e registrar as evidências em `docs/validacao-fundacao.md`.

## Acompanhamento

- Arquivar a change após a revisão e o aceite dos critérios da fundação.
- Iniciar a change `identidade-contas` somente depois que a fundação estiver
  verde.

## Critérios de aceite

- `docker compose up -d db` sobe o PostgreSQL saudável e
  `dotnet ef database update` aplica a migration inicial.
- `/health` responde saudável com o banco disponível e não saudável com o
  banco parado.
- `/swagger` lista o contrato da API em Development.
- O shell web carrega em `/`, exibe textos em PT-BR, funciona de 360 px a
  1440 px e mostra a atribuição à FreeToGame com link seguro.
- `dotnet test api/GameMatch.sln` e `npm test -- --watch=false` passam.
- Nenhum segredo real está versionado.
- Os cenários da seção **Requisitos** foram percorridos e registrados em
  `docs/validacao-fundacao.md`.

## Aprovação

- **Status:** Aprovado
- **Revisor:** Desenvolvedor responsável
- **Data:** 2026-11-10
- **Observações:** A change já está em execução (16/26 tarefas). Este PRD
  consolida o planejamento e o estado atual; a aprovação libera a conclusão das
  tarefas restantes e a verificação integrada.

> A implementação só continua quando o status for `Aprovado`.

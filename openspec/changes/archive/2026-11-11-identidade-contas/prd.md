# PRD - Identidade e contas

## Identificação

| Campo | Valor |
|---|---|
| Change | `identidade-contas` |
| Status do PRD | Pendente |
| Revisor | Desenvolvedor responsável |
| Revisado em | — |

> Estado atual: 16 de 16 tarefas. Este PRD consolida o planejamento da change
> "identidade e contas". A implementação só começa após a aprovação abaixo.

## Problema e objetivo

A fundação (`fundacao-projeto`) está arquivada e verde: monorepo, API em
camadas, PostgreSQL local, shell Angular, testes e documentação operacional.
Porém não existe conta de usuário: nada distingue visitante de usuário
autenticado, não há autor para ações futuras (favoritos, avaliações, histórico)
e nenhum endpoint da API é protegido.

**Objetivo:** entregar cadastro, login, sessão persistente com JWT e
gerenciamento básico de conta, no backend e no shell web, protegendo os
endpoints próprios da conta e deixando o cabeçalho consciente da sessão.

## Escopo

### O que muda

- `Domain`: entidade `Usuario` (Identity) e regras de conta.
- `Infrastructure`: pacote de stores do Identity, `GameMatchDbContext` derivado
  de `IdentityDbContext<Usuario>` e migration `IdentidadeInicial`.
- `Api`: `AddAuthentication`/`AddAuthorization` com JWT, `ContasController`
  (cadastro e login) e `ContaController` (autorizado), com a chave de assinatura
  vinda de variável de ambiente ou user-secrets.
- `web`: `SessaoService` com signal, interceptor de `Authorization`, guard de
  rota, telas `/entrar`, `/criar-conta` e `/conta`, cabeçalho com estado da
  sessão.
- Testes de backend e de frontend; documentação em `README.md` e `docs/`.

### Fora de escopo

- Perfil gamer (preferências de gênero, plataforma e estilo) — próxima change.
- Confirmação de e-mail, "esqueci a senha", login social/OAuth e refresh token.
- Papéis e autorizações por perfil, bloqueio de conta configurável e auditoria.

### Capacidades afetadas

- `identidade-contas` (nova): cadastro, login, sessão JWT e conta no web.
  Requisitos vivem neste PRD (`skip_specs: true`), sem spec em `openspec/specs`.

### Impacto

- **Novo código:** `api/src/GameMatch.Domain/Entidades/Usuario.cs`, controllers
  `Contas` e `Conta`, migration de identidade,
  `web/src/app/core/sessao/` e `web/src/app/features/{entrar,criar-conta,conta}/`.
- **Ferramentas:** pacotes
  `Microsoft.AspNetCore.Identity.EntityFrameworkCore` e
  `Microsoft.AspNetCore.Authentication.JwtBearer`, com versões fixadas nos
  `.csproj`.
- **Banco:** tabelas `AspNetUsers`, `AspNetRoles` etc. via migration; dados
  existentes (`registros_sistema`) preservados.
- **Segredos:** `Jwt:Chave` obrigatória por variável de ambiente ou
  user-secrets, nunca versionada.
- **Riscos operacionais:** migration do Identity com várias tabelas; token
  expirado tratado no web; `localStorage` exposto a XSS.

## Requisitos

### Requirement: Cadastro de conta

O sistema DEVE (MUST) criar contas com e-mail e senha, validando as regras de
senha e recusando e-mail já cadastrado.

#### Scenario: Cadastro válido

- **WHEN** um visitante envia um e-mail válido e uma senha que atende às regras
- **THEN** a API responde com sucesso, entrega um token JWT e o usuário fica
  autenticado

#### Scenario: Senha fora das regras

- **WHEN** a senha enviada não atende às regras configuradas
- **THEN** a API responde 400 com ProblemDetails listando as regras violadas

#### Scenario: E-mail já cadastrado

- **WHEN** o cadastro usa um e-mail já existente
- **THEN** a API responde 409 com ProblemDetails, sem vazar informação interna
  do banco

### Requirement: Autenticação por JWT

O sistema DEVE (MUST) autenticar por e-mail e senha e emitir um token JWT
assinado, com expiração configurável.

#### Scenario: Login válido

- **WHEN** credenciais corretas são enviadas ao login
- **THEN** a API responde com token e expiração

#### Scenario: Credenciais inválidas

- **WHEN** a senha ou o e-mail está incorreto
- **THEN** a API responde 401 e a mensagem não revela qual campo falhou

### Requirement: Autorização da conta

O sistema DEVE (MUST) exigir token válido para os endpoints próprios da conta.

#### Scenario: Chamada sem token

- **WHEN** o cliente acessa `/api/conta` sem o cabeçalho `Authorization`
- **THEN** a API responde 401

#### Scenario: Chamada com token válido

- **WHEN** o cliente envia `Authorization: Bearer <token>` válido
- **THEN** a API responde com os dados básicos do usuário autenticado

### Requirement: Gerenciamento de conta

O sistema DEVE (MUST) permitir visualizar os dados básicos e alterar a senha,
exigindo a senha atual.

#### Scenario: Alteração de senha correta

- **WHEN** o usuário autenticado envia a senha atual correta e uma nova senha
  válida
- **THEN** a senha é substituída e a API responde com sucesso

#### Scenario: Senha atual incorreta

- **WHEN** a senha atual informada não confere
- **THEN** a API responde erro de validação e a senha não é alterada

### Requirement: Sessão persistente no web

O sistema DEVE (MUST) manter a sessão no navegador e refletir o estado no
cabeçalho, em português.

#### Scenario: Recarregamento da página

- **WHEN** a página é recarregada com token válido salvo
- **THEN** a sessão é restabelecida e o cabeçalho exibe o estado autenticado

#### Scenario: Expiração do token

- **WHEN** o token salvo expirou
- **THEN** a sessão é encerrada, o token é removido e o cabeçalho volta a
  exibir a opção de entrar

#### Scenario: Saída da conta

- **WHEN** o usuário aciona "Sair"
- **THEN** o token é removido e a navegação volta ao estado visitante

### Requirement: Rotas protegidas

O sistema DEVE (MUST) proteger as rotas que exigem sessão.

#### Scenario: Acesso sem sessão

- **WHEN** `/conta` é aberta sem sessão válida
- **THEN** o navegador é redirecionado para `/entrar` preservando o destino

#### Scenario: Acesso com sessão

- **WHEN** `/conta` é aberta com sessão válida
- **THEN** a página renderiza os dados do usuário

### Requirement: Credenciais e segredos fora do repositório

O sistema DEVE (MUST) ler a chave de assinatura JWT de variável de ambiente ou
de user-secrets, e o repositório NÃO DEVE (MUST NOT) conter segredo real.

#### Scenario: Inicialização sem chave

- **WHEN** a API inicia sem `Jwt:Chave` configurada
- **THEN** a inicialização falha com mensagem que identifica a configuração
  ausente

#### Scenario: Varredura do repositório

- **WHEN** o conteúdo versionado é inspecionado
- **THEN** não há chave JWT real; `appsettings*.json` traz apenas placeholder

### Requirement: Testes automatizados executáveis

O projeto DEVE (MUST) manter as duas suítes verdes após as mudanças.

#### Scenario: Testes do backend

- **WHEN** `dotnet test api/GameMatch.sln` é executado
- **THEN** a suíte conclui com sucesso cobrindo cadastro, login, 401 e alteração
  de senha

#### Scenario: Testes do frontend

- **WHEN** `npm test -- --watch=false` é executado em `web/`
- **THEN** a suíte conclui com sucesso cobrindo sessão e guard

## Decisões técnicas

### 1. ASP.NET Core Identity + JWT

Escolha do desenvolvedor, alinhada ao `README.md` e a `docs/arquitetura.md`.
Identity entrega hash de senha, validação de regras e lockout sem código próprio;
a emissão do JWT continua manual, com claims `sub`, `email` e `name`.
Alternativa descartada: entidade própria com PBKDF2, que exigiria reimplementar
hash, validação e bloqueio.

### 2. `Usuario` no Domain, `IdentityDbContext` na Infrastructure

`Usuario : IdentityUser` vive em `GameMatch.Domain/Entidades`, mantendo o
domínio como fonte das regras de conta. O Domain passa a referenciar apenas
`Microsoft.Extensions.Identity.Stores` (abstrações), sem EF Core nem ASP.NET
Core; a Infrastructure referencia
`Microsoft.AspNetCore.Identity.EntityFrameworkCore` para derivar
`IdentityDbContext<Usuario>`. Trade-off assumido: o Domain deixa de ser
totalmente livre de dependências do Identity.

### 3. Chave de assinatura em segredo

`Jwt:Chave`, `Jwt:ExpiracaoMinutos`, `Jwt:Emissor` e `Jwt:Audience` ficam em
`appsettings.json` apenas como placeholder; o valor real vem de `Jwt__Chave`
(variável de ambiente) ou user-secrets, com falha de inicialização clara —
mesmo padrão já usado para `ConnectionStrings__GameMatch` em
`api/src/GameMatch.Api/Program.cs`.

### 4. Token em localStorage no Angular

`SessaoService` expõe um signal de usuário e grava o token em `localStorage`.
Um interceptor registrado em `provideHttpClient(withInterceptors(...))` adiciona
`Authorization: Bearer` às chamadas `/api` e limpa a sessão em respostas 401.
Alternativa descartada: cookie `httpOnly`, mais seguro, porém exige credenciais
no CORS e ajuste no proxy de desenvolvimento.

### 5. Rotas e guard em PT-BR

Rotas lazy `/entrar`, `/criar-conta` e `/conta`; `autenticadoGuard` devolve
`UrlTree` para `/entrar` quando não há sessão válida, preservando o destino
pretendido. Textos de interface em português; identificadores, métodos e
variáveis em português conforme `AGENTE.md`.

### 6. Regras de senha

`IdentityOptions.Password`: comprimento mínimo 8, ao menos 1 dígito, 1
maiúscula e 1 minúscula. As mensagens de erro do Identity são traduzidas para o
ProblemDetails de resposta.

### 7. Escopo enxuto por fase

Perfil gamer, confirmação de e-mail e refresh token ficam fora desta change para
mantê-la pequena, conforme `AGENTE.md` e o Acompanhamento da fundação.

## Riscos e trade-offs

- **Tabelas do Identity** → a migration cria cerca de cinco tabelas; documentar
  o conteúdo da migration e validar `dotnet ef database update` do zero.
- **Domain dependente de abstrações do Identity** → trade-off assumido na
  decisão 2; manter o Domain sem EF Core e sem ASP.NET Core.
- **Token em `localStorage`** → risco de XSS; aceito no MVP acadêmico e
  registrado explicitamente como trade-off.
- **Token expirado durante o uso** → o interceptor limpa a sessão e o guard
  redireciona para `/entrar`.
- **Migration aplicada sobre banco existente** → testar em banco vazio e no
  banco com `registros_sistema`, confirmando que os dados anteriores permanecem.
- **Cache de build no OneDrive** → mesmo risco da change anterior; limpar
  `bin/`, `obj/` e `.angular/` quando o build travar.

## Plano de execução e rollback

Ordem: backend (pacotes → entidade e contexto → configuração de autenticação →
endpoints de cadastro/login → endpoints de conta → testes), depois frontend
(sessão → interceptor → guard e rotas → telas → cabeçalho → testes) e, por fim,
verificação integrada e documentação.

Mudança aditiva: nova entidade, nova migration, novos controllers e novas rotas.
Rollback: reverter a migration (`dotnet ef database update <migration anterior>`),
remover controllers e rotas e desfazer os registros de autenticação. Não há dado
de usuário pré-existente a preservar.

## Tarefas

## 1. Backend — identidade

- [x] 1.1 Adicionar os pacotes `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  e `Microsoft.AspNetCore.Authentication.JwtBearer` aos projetos `Api` e
  `Infrastructure`; verificar `dotnet build api/GameMatch.sln`.
- [x] 1.2 Criar a entidade `Usuario : IdentityUser` no `Domain` e mudar
  `GameMatchDbContext` para `IdentityDbContext<Usuario>`; verificar
  `dotnet ef migrations add IdentidadeInicial` e `dotnet ef database update`
  criando as tabelas `AspNet*`.
- [x] 1.3 Configurar `AddIdentity`, `AddAuthentication` com JWT e a validação de
  `Jwt:Chave`; verificar que a API sem a chave falha com mensagem clara e que,
  com a chave configurada, inicia.
- [x] 1.4 Criar `ContasController` com `POST /api/contas` (cadastro) e
  `POST /api/contas/login`; verificar 201/200 com token, 400 para regras de
  senha, 409 para e-mail duplicado e 401 para credenciais inválidas.
- [x] 1.5 Criar `ContaController` autorizado com `GET /api/conta` e
  `PUT /api/conta/senha`; verificar 401 sem token, 200 com token e recusa de
  senha atual incorreta.
- [x] 1.6 Adicionar testes de integração de cadastro, login, autorização e
  alteração de senha; verificar `dotnet test api/GameMatch.sln` verde.

## 2. Frontend — sessão e contas

- [x] 2.1 Criar `SessaoService` com signal, gravação e remoção do token em
  `localStorage` e leitura da expiração; verificar round-trip e expiração em
  teste unitário.
- [x] 2.2 Criar o interceptor de `Authorization` e registrá-lo com
  `provideHttpClient(withInterceptors(...))`; verificar que `/api/conta` recebe o
  cabeçalho `Bearer`.
- [x] 2.3 Criar `autenticadoGuard` e as rotas lazy `/entrar`, `/criar-conta` e
  `/conta`; verificar redirecionamento sem sessão e renderização com sessão.
- [x] 2.4 Criar as telas de entrar e criar conta com formulários reativos e
  mensagens de erro em PT-BR; verificar cadastro e login no navegador.
- [x] 2.5 Criar a página de conta (dados, alterar senha, sair) e atualizar o
  cabeçalho com o estado da sessão; verificar responsividade em 360 e 1440 px
  sem rolagem horizontal.
- [x] 2.6 Adicionar testes de sessão, guard e cabeçalho; verificar
  `npm test -- --watch=false` verde.

## 3. Verificação integrada

- [x] 3.1 Executar o runbook do zero (banco, migrations, API, web) e percorrer os
  cenários no navegador; registrar evidências em
  `docs/validacao-identidade-contas.md`.
- [x] 3.2 Executar `dotnet test api/GameMatch.sln` e `npm test -- --watch=false`;
  verificar ambas as suítes verdes.
- [x] 3.3 Verificar que nenhum segredo está versionado, inspecionando
  `appsettings*.json`, `.env.example` e uma busca por chaves; verificar que
  apenas o placeholder de `Jwt:Chave` está no repositório.
- [x] 3.4 Documentar em `README.md` e `docs/ambiente.md` as rotas de conta, a
  variável `Jwt__Chave` e os comandos; verificar que os comandos documentados
  executam como escritos.

## Acompanhamento

- Arquivar a change após revisão e aceite dos critérios.
- Iniciar a change `perfil-gamer` somente depois desta estar verde.

## Critérios de aceite

- Cadastro e login entregam token; `/api/conta` responde 401 sem token e 200
  com token.
- O web mantém a sessão após recarregar, protege `/conta` e reflete o estado no
  cabeçalho.
- Alterar senha exige a senha atual correta.
- `dotnet test api/GameMatch.sln` e `npm test -- --watch=false` passam.
- Nenhuma chave JWT real está versionada.

## Aprovação

- **Status:** Aprovado
- **Revisor:** Desenvolvedor responsável
- **Data:** —
- **Observações:** Aguardando revisão do PRD antes da implementação.

> A implementação só começa quando o status for `Aprovado`.

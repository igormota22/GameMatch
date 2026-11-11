# Ambiente de desenvolvimento

Guia operacional para subir o PostgreSQL, aplicar as migrations, executar a API
e rodar os testes. A visão de arquitetura está em
[arquitetura.md](arquitetura.md).

## Pré-requisitos

| Ferramenta | Versão de referência | Verificação |
|---|---|---|
| .NET SDK | 10.0.x | `dotnet --version` |
| Docker | 29.x | `docker --version` |
| Node.js | 24.x | `node --version` |
| npm | 11.x | `npm --version` |

Confira tudo de uma vez:

```powershell
dotnet --version
docker --version
node --version
npm --version
```

A ferramenta `dotnet-ef` é local ao repositório e fica registrada em
`dotnet-tools.json`; não é necessário instalá-la globalmente.

## Variáveis de ambiente

| Variável | Onde é usada | Obrigatória |
|---|---|---|
| `POSTGRES_DB` | `docker-compose.yml` | Sim, via `.env` |
| `POSTGRES_USER` | `docker-compose.yml` | Sim, via `.env` |
| `POSTGRES_PASSWORD` | `docker-compose.yml` | Sim, via `.env` |
| `POSTGRES_PORT` | `docker-compose.yml` | Não; padrão `5432` |
| `ConnectionStrings__GameMatch` | API | Sim, exceto se usar user-secrets |
| `ConnectionStrings__GameMatchTeste` | Fallback dos testes de integração | Não |
| `Cors__AllowedOrigins__0` | API | Não; padrão `http://localhost:4200` |
| `ASPNETCORE_ENVIRONMENT` | API | Não; o perfil `http` define `Development` |

O arquivo `.env` é ignorado pelo git. Apenas `.env.example`, com placeholders,
é versionado.

## 1. Subir o banco

Copie o arquivo de exemplo e defina uma senha local:

```powershell
Copy-Item .env.example .env
# edite .env e troque POSTGRES_PASSWORD
docker compose up -d db
docker compose ps
```

Em Linux ou macOS, use `cp .env.example .env` no lugar de `Copy-Item`.

O serviço deve aparecer como `Up (healthy)`:

```
NAME             SERVICE   STATUS
gamematch-db-1   db        Up (healthy)
```

## 2. Configurar a conexão da API

A API lê `ConnectionStrings:GameMatch` de variável de ambiente ou de
user-secrets. Use a mesma senha definida em `POSTGRES_PASSWORD`.

PowerShell:

```powershell
$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha definida no .env>"
```

Bash:

```bash
export ConnectionStrings__GameMatch="Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha definida no .env>"
```

Alternativa com user-secrets, válida apenas em Development:

```powershell
dotnet user-secrets set "ConnectionStrings:GameMatch" "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha definida no .env>" --project api/src/GameMatch.Api
```

Sem a configuração, a API encerra na inicialização com uma mensagem que
identifica a variável ausente.

## 3. Aplicar as migrations

```powershell
dotnet tool restore
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
```

O comando cria as tabelas `registros_sistema` e `__EFMigrationsHistory` no banco
`gamematch`.

## 4. Executar a API

```powershell
dotnet run --project api/src/GameMatch.Api --launch-profile http
```

A API responde em `http://localhost:5080`:

| Recurso | URL | Ambiente |
|---|---|---|
| Saúde | `http://localhost:5080/health` | Todos |
| Swagger UI | `http://localhost:5080/swagger` | Development |
| Documento OpenAPI | `http://localhost:5080/openapi/v1.json` | Development |

Encerre com `Ctrl+C`.

## 5. Executar os testes

```powershell
dotnet test api/GameMatch.sln
```

Os testes de integração usam Testcontainers quando o Docker está disponível.
Se o Docker não estiver acessível, defina `ConnectionStrings__GameMatchTeste`
com um banco PostgreSQL dedicado; na ausência das duas opções, o teste tenta
criar o banco `gamematch_teste` no PostgreSQL local.

Os testes do frontend usam o runner padrão do Angular CLI:

```powershell
cd web
npm test -- --watch=false
```

## Comandos EF úteis

```powershell
# Listar migrations
dotnet ef migrations list --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api

# Criar uma nova migration
dotnet ef migrations add NomeDaMigration --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api --output-dir Persistencia/Migracoes

# Aplicar migrations pendentes
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api

# Reverter para uma migration específica (ou para nenhuma)
dotnet ef database update NomeDaMigration --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
```

Os comandos que inicializam a API precisam da string de conexão configurada.

## Portas

| Serviço | Porta padrão | Como alterar |
|---|---|---|
| API | `5080` | `Properties/launchSettings.json` ou `ASPNETCORE_URLS` |
| PostgreSQL | `5432` | `POSTGRES_PORT` no `.env` |
| Web | `4200` | `npm start -- --port <porta>` ou `angular.json` |

## CORS e correlação

- A API aceita CORS apenas para `http://localhost:4200`. Para outra origem,
  defina `Cors__AllowedOrigins__0`.
- Envie `X-Correlation-ID` para correlacionar a requisição; a API devolve o
  mesmo valor no cabeçalho e o inclui no escopo dos logs.
- Respostas de erro seguem ProblemDetails (RFC 7807) com `traceId`,
  `application/problem+json` e sem stack trace em produção.

## Frontend e Tailwind

O app Angular fica em `web/` e usa Tailwind CSS 4 via PostCSS:

```powershell
cd web
npm install
npm start
```

O dev server responde em `http://localhost:4200`. Para servir a aplicação com a
API em execução:

```powershell
# em outro terminal, com o banco e a API ativos
cd web
npm start
```

### Proxy de desenvolvimento

`web/proxy.conf.json` encaminha `/api` para `http://localhost:5080` e remove o
prefixo antes de repassar. Assim, o frontend chama `/api/health` e a API recebe
`/health`; o CORS não é acionado porque a requisição sai do próprio dev server.

```json
{
  "/api": {
    "target": "http://localhost:5080",
    "secure": false,
    "changeOrigin": true,
    "pathRewrite": { "^/api": "" }
  }
}
```

O `angular.json` referencia esse arquivo em
`projects.gamematch-web.architect.serve.options.proxyConfig`. Alterações no
proxy exigem reiniciar o `npm start`. Se a API estiver parada, o shell carrega
normalmente e a página inicial informa que a API não está disponível.

Arquivos de integração:

- `web/.postcssrc.json` registra o plugin `@tailwindcss/postcss`.
- `web/src/styles.css` contém `@import "tailwindcss";`.
- As classes utilitárias são detectadas automaticamente nos templates Angular.

Se o builder do Angular rejeitar o Tailwind 4, o fallback documentado é o
Tailwind 3: instale `tailwindcss@3 postcss autoprefixer`, crie
`tailwind.config.js` com `content: ["./src/**/*.{html,ts}"]`, use
`@tailwind base; @tailwind components; @tailwind utilities;` em `styles.css` e
registre `tailwindcss` e `autoprefixer` em `.postcssrc.json`.

## Problemas comuns

| Sintoma | Causa provável | Correção |
|---|---|---|
| `failed to connect to the docker API` | Docker Desktop parado | Inicie o Docker Desktop e aguarde o daemon |
| `password authentication failed` | Senha do `.env` diferente da string de conexão | Alinhe a senha; se o volume foi criado com outra, recrie com `docker compose down -v` |
| Porta `5432` ocupada | Outro PostgreSQL local | Troque `POSTGRES_PORT` no `.env` e na string de conexão |
| Builds travando | Sincronização do OneDrive | Pause a sincronização ou mova o projeto para `C:\dev\GameMatch` |
| Testcontainers indisponível | Docker parado | Inicie o Docker ou defina `ConnectionStrings__GameMatchTeste` |

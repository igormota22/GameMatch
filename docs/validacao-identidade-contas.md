# Validação integrada — identidade e contas

Registro da execução do runbook da change `identidade-contas` do zero, com
banco recriado, migrations aplicadas, API e web em execução e cenários
percorridos no navegador.

- **Data:** 11 de novembro de 2026
- **Ambiente:** Windows, .NET SDK 10.0.103, Node.js 24.21.0, npm 11.19.0,
  Docker Desktop com `gamematch-db-1` saudável
- **Banco:** PostgreSQL local (`gamematch`), recriado com
  `dotnet ef database drop --force` seguido de `dotnet ef database update`

## 1. Banco e migrations

```powershell
$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha local>"
dotnet ef database drop --force --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
```

Resultado: banco recriado do zero e migration `20261111042240_IdentidadeInicial`
aplicada. Tabelas presentes:

| Tabela | Situação |
|---|---|
| `AspNetUsers` | criada |
| `AspNetRoles` | criada |
| `AspNetUserRoles` | criada |
| `AspNetUserClaims` | criada |
| `AspNetRoleClaims` | criada |
| `AspNetUserLogins` | criada |
| `AspNetUserTokens` | criada |
| `registros_sistema` | preservada |
| `__EFMigrationsHistory` | criada |

## 2. API e web

```powershell
# terminal 1
$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha local>"
dotnet run --project api/src/GameMatch.Api --launch-profile http

# terminal 2
cd web
npm start
```

Resultados:

| Verificação | Resultado |
|---|---|
| `GET http://localhost:5080/health` | `200` com `{"status":"Healthy"}` e PostgreSQL saudável |
| `http://localhost:4200` | aplicação carregada em `GameMatch — Início` |
| API sem `Jwt:Chave` | encerra na inicialização com mensagem em português apontando `Jwt__Chave` |
| API com `Jwt__Chave` (user-secrets em Development) | inicia normalmente |

## 3. Cenários no navegador

Usuário de teste: `runbook.<timestamp>@teste.com` (criado durante a execução).

| # | Cenário | Resultado observado |
|---|---|---|
| 1 | Visitante acessa `/conta` | redireciona para `/entrar?destino=%2Fconta`; título `GameMatch — Entrar` |
| 2 | Cadastro em `/criar-conta` com e-mail novo e senha `Senha123` | `201` na API, login automático, redireciona para `/conta` |
| 3 | Cabeçalho após cadastro | exibe `Minha conta`, e-mail do usuário e `Sair` |
| 4 | Recarregar `/conta` | sessão permanece ativa; dados da conta continuam visíveis |
| 5 | Alterar senha para `NovaSenha123` | mensagem `Senha alterada com sucesso.` |
| 6 | Sair pelo cabeçalho | volta para `/`, sessão removida, cabeçalho exibe `Entrar` e `Criar conta` |
| 7 | Login com a senha antiga `Senha123` | permanece em `/entrar` com `E-mail ou senha incorretos.` |
| 8 | Login com a nova senha `NovaSenha123` | redireciona para `/conta` e exibe o e-mail |
| 9 | Responsividade em 360 px | `scrollWidth` = `clientWidth` = 345 px (sem rolagem horizontal) |
| 10 | Responsividade em 1440 px | `scrollWidth` = `clientWidth` = 1440 px (sem rolagem horizontal) |

## 4. Endpoints verificados por HTTP

| Requisição | Resultado |
|---|---|
| `POST /contas` (dados válidos) | `201` com `token`, `expiraEm` e `usuario` |
| `POST /contas` (e-mail duplicado) | `409` com ProblemDetails em PT-BR |
| `POST /contas` (senha fraca) | `400` com mensagens das regras de senha |
| `POST /contas/login` (credenciais válidas) | `200` com token |
| `POST /contas/login` (credenciais inválidas) | `401` com mensagem genérica |
| `GET /conta` (sem token) | `401` |
| `GET /conta` (com token) | `200` com e-mail e data de criação |
| `PUT /conta/senha` (senha atual incorreta) | `400` |
| `PUT /conta/senha` (senha atual correta) | `204`; login seguinte usa a nova senha |

## 5. Suítes automatizadas

```powershell
dotnet test api/GameMatch.sln
cd web
npm test -- --watch=false
```

- Backend: 1 teste unitário + 12 testes de integração aprovados.
- Frontend: 5 arquivos, 14 testes aprovados.

## Conclusão

O runbook do zero foi concluído com migrations aplicadas, API e web em
execução, todos os cenários de conta funcionando e sem rolagem horizontal em
360 px e 1440 px.

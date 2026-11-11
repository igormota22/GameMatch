# Validação da fundação

Registro das verificações executadas para aceitar a change `fundacao-projeto`.
Ambiente: Windows, .NET 10.0.103, Node 24.21, Angular CLI 22.2.4, Docker 29.7.2
e PostgreSQL 17 em container. Data da execução: 2026-11-11.

## 1. Ambiente local reproduzível

Comandos executados a partir da raiz do repositório:

```powershell
Copy-Item .env.example .env   # .env já existente no ambiente local
docker compose up -d db
docker compose ps

$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha do .env>"
dotnet tool restore
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
dotnet run --project api/src/GameMatch.Api --launch-profile http

cd web
npm install
npm start
```

Resultados observados:

- `docker compose ps`: `gamematch-db-1` com status `Up (healthy)` e porta
  `5432` publicada.
- `dotnet ef database update`: banco atualizado; migration
  `20261110000329_Inicial` registrada em `__EFMigrationsHistory`.
- `\dt` no banco `gamematch`: tabelas `registros_sistema` e
  `__EFMigrationsHistory`.
- API em `http://localhost:5080`; web em `http://localhost:4200`.

> Observação: o repositório ainda não possui commit inicial, então não foi
> possível clonar um checkout limpo. A verificação foi feita a partir da árvore
> de trabalho com os caches de build removidos (`bin/`, `obj/`, `.angular/` e
> cache do NuGet) e reconstruídos pelos comandos acima.

## 2. Saúde e conectividade com o banco

### Banco disponível

```powershell
curl.exe -i http://localhost:5080/health
```

Resposta observada: `HTTP/1.1 200 OK` com corpo:

```json
{
  "status": "Healthy",
  "duracaoTotalMs": 471.0537,
  "verificacoes": {
    "postgresql": {
      "status": "Healthy",
      "descricao": "Conexão com o PostgreSQL bem-sucedida.",
      "erro": null,
      "duracaoMs": 469.2996
    }
  }
}
```

### Banco indisponível

Com a API em execução e o PostgreSQL inacessível (container parado/porta
fechada), a mesma consulta retornou `HTTP/1.1 503 Service Unavailable`:

```json
{
  "status": "Unhealthy",
  "duracaoTotalMs": 5831.5367,
  "verificacoes": {
    "postgresql": {
      "status": "Unhealthy",
      "descricao": "Não foi possível conectar ao PostgreSQL.",
      "erro": null,
      "duracaoMs": 5814.0021
    }
  }
}
```

Os logs da API confirmam a transição `503 Unhealthy` → `200 Healthy` quando o
banco passou a responder.

## 3. Documentação do contrato HTTP

```powershell
curl.exe -o NUL -w "%{http_code}" http://localhost:5080/swagger/index.html
curl.exe -o NUL -w "%{http_code}" http://localhost:5080/openapi/v1.json
```

Resultado: `200` para a Swagger UI e `200` para o documento OpenAPI. A UI lista o
endpoint `/health` e o schema da resposta.

## 4. Shell web responsivo em português

Verificado no navegador (Playwright) com a API e o banco ativos:

- Título: `GameMatch — Início`; `<html lang="pt-BR">`.
- Cabeçalho com navegação "Início", conteúdo principal e rodapé.
- Status exibido: `API e banco operacionais (Healthy).` — confirma a chamada do
  frontend ao `/health` pelo proxy.
- Rota inexistente (`/rota-inexistente`): título
  `GameMatch — Página não encontrada`, conteúdo "ERRO 404 — Página não
  encontrada" e link "Voltar para o início".

Larguras verificadas (`scrollWidth === clientWidth` indica ausência de rolagem
horizontal):

| Viewport | scrollWidth | clientWidth | Rolagem horizontal |
|---|---|---|---|
| 360 px | 360 | 360 | Não |
| 768 px | 768 | 768 | Não |
| 1440 px | 1440 | 1440 | Não |

Em 1440 px o conteúdo principal ocupa `1152 px`, limitado por `max-w-6xl` e
centralizado.

## 5. Proxy de desenvolvimento

```powershell
curl.exe -i http://localhost:4200/api/health
```

Resultado: a resposta é a mesma do endpoint `/health` da API (incluindo
`X-Correlation-ID` e o JSON de saúde), comprovando que `web/proxy.conf.json`
remove o prefixo `/api` e encaminha para `http://localhost:5080`. A página
inicial também exibe o estado saudável obtido por essa chamada.

## 6. CORS

```powershell
# Origem permitida
curl.exe -D - -o NUL -H "Origin: http://localhost:4200" http://localhost:5080/health

# Origem não permitida
curl.exe -D - -o NUL -H "Origin: http://origem-nao-permitida.example" http://localhost:5080/health
```

Resultados:

- Origem permitida: `Access-Control-Allow-Origin: http://localhost:4200`.
- Origem não permitida: nenhum cabeçalho `Access-Control-Allow-*` na resposta.

## 7. Erros padronizados (ProblemDetails)

Cobertura automatizada em `api/tests/GameMatch.IntegrationTests/ProblemDetailsTests.cs`:

| Cenário | Teste | Resultado |
|---|---|---|
| Requisição inválida (400) | `Requisicao_invalida_retorna_problemdetails_400` | `application/problem+json`, `status=400`, `traceId` e `errors.Nome` |
| Falha inesperada (500) | `Falha_inesperada_retorna_problemdetails_sem_stack_trace` | `application/problem+json`, `status=500`, `traceId`, sem stack trace |
| Rota inexistente (404) | `Rota_inexistente_retorna_problemdetails_com_traceid` | `application/problem+json`, `status=404`, `traceId` |

O endpoint de 400 é um controller exclusivo do projeto de testes
(`ValidacaoController`), seguindo o mesmo padrão do `ErroController`; nenhum
endpoint de teste é exposto pela aplicação.

## 8. Atribuição à FreeToGame

Verificado no rodapé renderizado:

- Texto: `Dados fornecidos por FreeToGame.com`.
- `href`: `https://www.freetogame.com`.
- `target`: `_blank`.
- `rel`: `noopener noreferrer`.

## 9. Testes automatizados

```powershell
dotnet test api/GameMatch.sln
cd web
npm test -- --watch=false
```

Resultados:

- `GameMatch.UnitTests`: 1 teste aprovado.
- `GameMatch.IntegrationTests`: 4 testes aprovados (health com banco, 400, 404 e
  500).
- Frontend (`ng test`/Vitest): 3 testes aprovados.

## 10. Varredura de segredos

- `git check-ignore -v .env` confirma que `.env` é ignorado
  (`.gitignore:54:.env`); `git ls-files` não lista `.env`.
- `.env.example` contém apenas placeholders, como
  `POSTGRES_PASSWORD=troque-por-uma-senha-local`.
- `docker-compose.yml` exige `POSTGRES_PASSWORD` via `.env`
  (`${POSTGRES_PASSWORD:?Defina POSTGRES_PASSWORD no arquivo .env}`).
- `appsettings*.json` não contêm strings de conexão nem chaves.
- As credenciais presentes em código de teste (`Password=teste` e o fallback
  local `gamematch-dev`) são efêmeras, restritas ao ambiente de teste, e não
  correspondem a segredo de produção.

## Conclusão

Todos os cenários da seção **Requisitos** do PRD foram percorridos e
registrados. As suítes de backend e frontend estão verdes e nenhum segredo real
está versionado.

# Arquitetura do GameMatch

Este documento descreve a organização técnica da fundação do GameMatch e as
decisões que as fases seguintes devem respeitar. A motivação do produto está no
[README](../README.md); as decisões completas da fundação estão no
[design do change `fundacao-projeto`](../openspec/changes/fundacao-projeto/design.md).

## Visão geral

O GameMatch é um monorepo com três blocos executáveis:

- **API** em ASP.NET Core (.NET 10), organizada em quatro camadas.
- **Web** em Angular 22, zoneless, com componentes standalone e signals.
- **Banco** PostgreSQL executado localmente via Docker Compose.

O catálogo da FreeToGame será sincronizado para o PostgreSQL a partir de um
snapshot local em [`seed/`](../seed/README.md), para que a demonstração funcione
sem depender da API externa. No MVP não há chamada em tempo real à FreeToGame:
o catálogo é carregado por um comando manual de sincronização (ver
[DT-11](tdd.md)). Os requisitos de produto estão em
[`docs/prd.md`](prd.md) e as decisões técnicas em [`docs/tdd.md`](tdd.md).

## Estrutura do monorepo

```
GameMatch/
+-- api/
|   +-- GameMatch.sln
|   +-- src/
|   |   +-- GameMatch.Domain/
|   |   +-- GameMatch.Application/
|   |   +-- GameMatch.Infrastructure/
|   |   +-- GameMatch.Api/
|   +-- tests/
|       +-- GameMatch.UnitTests/
|       +-- GameMatch.IntegrationTests/
+-- web/                     # aplicação Angular
+-- seed/                    # snapshot local do catálogo
+-- docs/                    # documentação técnica e operacional
+-- openspec/                # mudanças e especificações do projeto
+-- Directory.Build.props    # propriedades MSBuild comuns
+-- Directory.Packages.props # versões centralizadas de pacotes NuGet
```

## Camadas da API

```
+-------------------------+
|      GameMatch.Api      |  controllers, composição, Swagger, CORS
+------------+------------+
             |
             v
+-------------------------+
|  GameMatch.Infrastructure |  EF Core, Npgsql, acesso externo
+------------+------------+
             |
             v
+-------------------------+
|  GameMatch.Application  |  casos de uso, DTOs, validação, interfaces
+------------+------------+
             |
             v
+-------------------------+
|     GameMatch.Domain    |  entidades, objetos de valor, regras puras
+-------------------------+
```

Regra de dependência: as setas apontam sempre para dentro. `Domain` não
referencia nenhum outro projeto; `Application` referencia apenas `Domain`;
`Infrastructure` referencia `Application` e `Domain`; `Api` referencia as três
camadas e é o único ponto de composição da aplicação.

| Camada | Responsabilidade | Não pode conter |
|---|---|---|
| `Domain` | Entidades, objetos de valor e, na fase de recomendações, a calculadora de compatibilidade | EF Core, HTTP, ASP.NET Core |
| `Application` | Casos de uso, DTOs, validação e interfaces de repositório | Detalhes de banco ou de framework web |
| `Infrastructure` | `DbContext`, migrations, repositórios e clientes externos | Regras de negócio |
| `Api` | Controllers, autenticação, Swagger, CORS e injeção de dependência | Regras de negócio e SQL |

## Camadas do frontend

```
web/src/app/
+-- core/      # configuração, HTTP, guards e serviços globais
+-- shared/    # componentes reutilizáveis (botão, card, estado vazio)
+-- layout/    # shell, header e footer
+-- features/  # uma pasta por funcionalidade
```

As rotas são carregadas de forma lazy por feature. Hoje existem as rotas
públicas `/`, `/entrar` e `/criar-conta`, a rota autenticada `/conta` e a rota
404. As próximas features (perfil gamer, catálogo, recomendações, favoritos e
avaliações) seguem o roadmap do [TDD](tdd.md).

## Decisões da fundação

| Decisão | Escolha | Motivo resumido |
|---|---|---|
| Plataforma | .NET 10 + ASP.NET Core com controllers | Contrato HTTP explícito e legível no Swagger |
| Banco | PostgreSQL em Docker Compose | Banco relacional próximo do cenário de produção |
| Persistência | EF Core + Npgsql com migrations | Pipeline reproduzível de schema |
| Frontend | Angular 22 zoneless, standalone e signals | Exigência do projeto e estado simples sem NgRx |
| Estilo | Tailwind CSS | Produtividade e consistência visual |
| Ícones | Lucide com importação seletiva | Bundle menor que importar o pacote inteiro |
| Erros | ProblemDetails (RFC 7807) com `traceId` | Formato padrão e rastreável |
| Health | `/health` com verificação do PostgreSQL | Diagnóstico rápido do ambiente local |
| OpenAPI | Documento nativo + Swagger UI em desenvolvimento | Inspeção do contrato sem ferramentas externas |
| CORS | Origens explícitas por configuração | Evitar mascarar problemas de integração |
| Segredos | Variáveis de ambiente ou user-secrets | Nada sensível versionado |
| Testes | xUnit no backend e runner do Angular CLI no frontend | Suítes executáveis desde a fundação |
| Dados | Snapshot local da FreeToGame em `seed/` | Demo offline e reproduzível |
| Atribuição | Rodapé com link ativo para FreeToGame.com | Exigência dos termos da API |

Versões de pacotes NuGet ficam centralizadas em
[`Directory.Packages.props`](../Directory.Packages.props), e propriedades comuns
de compilação em [`Directory.Build.props`](../Directory.Build.props).

## Ambiente local

| Serviço | Porta padrão | Observação |
|---|---|---|
| API | `5080` | Configurável em `launchSettings.json` ou variável de ambiente |
| Web | `4200` | Servidor de desenvolvimento do Angular CLI |
| PostgreSQL | `5432` | Sobrescrevível por `.env` |

A string de conexão da API é lida de `ConnectionStrings:GameMatch`, por variável
de ambiente ou user-secrets. O frontend encaminha `/api` para a API por meio de
um proxy de desenvolvimento, evitando CORS artificial.

O passo a passo operacional completo está em [`docs/ambiente.md`](ambiente.md).
Para compilar a solução:

```bash
dotnet build api/GameMatch.sln
```

## Evolução por fases

O roadmap técnico detalhado, com pré-requisitos, entregáveis e critérios de
saída, está na seção 8 do [TDD](tdd.md):

| Fase | Escopo | Prioridade |
|---|---|---|
| F1 | Perfil gamer | P0 |
| F2 | Catálogo e sincronização do snapshot | P0 |
| F3 | Recomendações e Modo Descoberta | P1 |
| F4 | Favoritos e avaliações | P1 |
| F5 | Verificação do MVP | — |

As fases já concluídas (fundação e identidade/contas) estão nas changes
arquivadas em `openspec/changes/archive/`. Cada fase nova é uma change OpenSpec
própria (proposal, specs, design quando necessário e tasks verificáveis),
conforme o fluxo descrito em `openspec/config.yaml`.

# GameMatch 🎮

## Sobre o projeto

O **GameMatch** é uma plataforma web de descoberta e recomendação personalizada de jogos gratuitos, desenvolvida para ajudar jogadores a encontrar novos títulos de acordo com seus gostos, preferências e estilos de gameplay.

A aplicação utiliza informações de jogos disponibilizadas pela API FreeToGame e combina esses dados com um sistema próprio de recomendação, buscando apresentar jogos que realmente correspondam ao perfil de cada usuário.

## Objetivo

Desenvolver uma plataforma que facilite a descoberta de jogos gratuitos, oferecendo recomendações personalizadas com base nas preferências individuais dos jogadores.

O principal diferencial do GameMatch é apresentar não apenas uma lista de jogos populares, mas também explicar por que determinado jogo combina com o perfil do usuário.

## Funcionalidades principais

- **Autenticação:** cadastro, login e gerenciamento de conta.
- **Perfil gamer:** configuração das preferências e características do jogador.
- **Catálogo de jogos:** exploração dos jogos gratuitos disponíveis na plataforma.
- **Pesquisa e filtros:** busca por nome, gênero, plataforma e outras características.
- **Recomendações personalizadas:** sugestões de jogos baseadas nas preferências do usuário.
- **Compatibilidade:** pontuação de compatibilidade entre o jogador e cada jogo.
- **Explicação das recomendações:** apresentação dos critérios que contribuíram para cada sugestão.
- **Modo Descoberta:** recomendações baseadas na experiência que o jogador deseja encontrar no momento.
- **Favoritos:** gerenciamento de jogos de interesse.
- **Avaliações:** registro de avaliações pessoais dos jogos.
- **Histórico:** acompanhamento dos jogos avaliados e explorados (evolução
  pós-MVP).

## Público-alvo

Jogadores que desejam descobrir novos jogos gratuitos, encontrar experiências compatíveis com suas preferências e explorar diferentes gêneros e estilos de gameplay.

## Tecnologias utilizadas

### Backend
- C#
- .NET / ASP.NET Core Web API
- Entity Framework Core
- Banco de dados relacional
- ASP.NET Core Identity
- JWT

### Frontend
- Angular
- TypeScript
- Tailwind CSS

### Integrações
- **FreeToGame API:** obtenção de informações sobre jogos gratuitos, gêneros, plataformas e desenvolvedores.

## Arquitetura e documentação

- [PRD](docs/prd.md): requisitos de produto, regras de negócio e critérios de
  aceite do MVP.
- [TDD](docs/tdd.md): decisões técnicas, contratos, dados, testes e roadmap de
  implementação.
- [Arquitetura da fundação](docs/arquitetura.md): camadas da API, estrutura do
  monorepo, decisões técnicas e portas locais.
- [Snapshot do catálogo](seed/README.md): formato previsto dos dados offline da
  FreeToGame.

## Como executar

O guia completo, com variáveis de ambiente, portas e solução de problemas, está
em [docs/ambiente.md](docs/ambiente.md).

### Pré-requisitos

| Ferramenta | Versão de referência |
|---|---|
| .NET SDK | 10.0.x |
| Docker | 29.x |
| Node.js | 24.x |
| npm | 11.x |

Confira as versões instaladas:

```powershell
dotnet --version
docker --version
node --version
npm --version
```

### Subir o banco

```powershell
Copy-Item .env.example .env
# edite .env e defina POSTGRES_PASSWORD
docker compose up -d db
docker compose ps
```

### Aplicar migrations e executar a API

```powershell
$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=<senha definida no .env>"
dotnet tool restore
dotnet user-secrets set "Jwt:Chave" "<chave com pelo menos 32 caracteres>" --project api/src/GameMatch.Api
dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api
dotnet run --project api/src/GameMatch.Api --launch-profile http
```

A chave JWT é obrigatória e não é versionada: `appsettings.json` traz apenas o
placeholder vazio. Em Development, use o `dotnet user-secrets` acima; em outros
ambientes, defina `Jwt__Chave`. Sem a chave, a API encerra na inicialização com
uma mensagem em português.

A API responde em `http://localhost:5080`. Verifique
`http://localhost:5080/health` e, em Development,
`http://localhost:5080/swagger`.

### Executar o frontend

```powershell
cd web
npm install
npm start
```

O app Angular responde em `http://localhost:4200`. Durante o desenvolvimento, o
dev server encaminha chamadas a `/api/*` para a API em `http://localhost:5080`,
removendo o prefixo `/api` (por exemplo, `/api/health` chega como `/health`).
A configuração fica em `web/proxy.conf.json` e é aplicada por
`web/angular.json`.

Com a API parada, o shell continua carregando e exibe o status da API como
indisponível; nenhuma chamada quebra a navegação.

### Contas e sessão

Rotas do frontend:

| Rota | Acesso | Conteúdo |
|---|---|---|
| `/entrar` | Público | Login com e-mail e senha |
| `/criar-conta` | Público | Cadastro com login automático |
| `/conta` | Autenticado | Dados da conta, alteração de senha e saída |
| `/perfil` | Autenticado | Preferências do perfil gamer em modo de leitura e edição |
| `/catalogo` | Autenticado | Catálogo com pesquisa, filtros de gênero, plataforma e tag e paginação |
| `/recomendacoes` | Autenticado | Recomendações personalizadas com pontuação, fatores explicados e Modo Descoberta |

Endpoints da API:

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/contas` | Cria a conta e devolve o token |
| `POST` | `/contas/login` | Autentica e devolve o token |
| `GET` | `/conta` | Dados da conta autenticada |
| `PUT` | `/conta/senha` | Altera a senha, exigindo a senha atual |
| `GET` | `/perfil` | Preferências do perfil gamer autenticado |
| `PUT` | `/perfil` | Cria ou substitui as preferências do perfil |
| `GET` | `/catalogo` | Catálogo paginado com busca e filtros |
| `GET` | `/catalogo/filtros` | Gêneros, plataformas e tags disponíveis no catálogo |
| `GET` | `/catalogo/{id}` | Detalhe do jogo com características e URL da fonte |
| `GET` | `/recomendacoes?experiencia=` | Top 12 recomendações explicadas (409 se o perfil está incompleto, 404 se `experiencia` é desconhecida) |
| `GET` | `/recomendacoes/opcoes` | Opções de experiência do Modo Descoberta (chave e rótulo) |

O catálogo é populado a partir do snapshot versionado em `seed/` pelos comandos
`dotnet run --project api/src/GameMatch.Seed -- coletar` (gera o snapshot a
partir da FreeToGame) e `dotnet run --project api/src/GameMatch.Seed -- sync`
(upsert idempotente no banco). A aplicação nunca escreve em `seed/`; consulte
[`seed/README.md`](seed/README.md) e
[`docs/ambiente.md`](docs/ambiente.md) para o procedimento completo.

O token fica em `localStorage`, é enviado pelo interceptor como
`Authorization: Bearer <token>` e o guard redireciona visitantes de `/conta`,
`/perfil`, `/catalogo` e `/recomendacoes` para `/entrar` preservando o destino. A sessão persiste
após recarregar a página. Após o login sem destino informado, o usuário volta
para a home, que exibe o resumo do perfil.

### Executar os testes

```powershell
dotnet test api/GameMatch.sln
cd web
npm test -- --watch=false
```

## Sistema de recomendação

O GameMatch usa um algoritmo próprio de recomendação baseado nas preferências
cadastradas pelo jogador. A compatibilidade é calculada considerando
características como:

- Gêneros favoritos.
- Plataformas de interesse.
- Estilos de gameplay.
- No Modo Descoberta, a experiência escolhida no momento da consulta.

Cada recomendação traz uma pontuação de compatibilidade, acompanhada da
explicação dos fatores que contribuíram para o resultado. A fórmula, os pesos e
os contratos estão em [docs/tdd.md](docs/tdd.md).

O sistema está disponível em `/recomendacoes` (F3): a lista usa o topo de 12
jogos do catálogo e o seletor do Modo Descoberta adiciona o fator de
experiência à pontuação. Jogos favoritados ou avaliados pelo usuário ficam
fora das recomendações (DT-12/INV-06).

A tela `/favoritos` (F4) reúne os jogos marcados, permite removê-los da lista e
avaliar cada um de 1 a 5 estrelas, com alteração e remoção da avaliação. O
botão de favoritar também aparece nos cards do catálogo e das recomendações.

## Escopo inicial (MVP)

A primeira versão da aplicação contempla:

- Cadastro e autenticação de usuários.
- Configuração do perfil gamer.
- Catálogo sincronizado a partir do snapshot local da FreeToGame.
- Catálogo com pesquisa e filtros.
- Sistema de recomendações personalizadas.
- Pontuação e explicação de compatibilidade.
- Modo Descoberta.
- Gerenciamento de jogos favoritos.
- Avaliação de jogos.

## Status do projeto

🚧 Em desenvolvimento.

Projeto acadêmico com previsão de entrega de uma versão funcional até novembro de 2026.

## Finalidade

Projeto desenvolvido para fins acadêmicos, com o objetivo de aplicar conhecimentos de desenvolvimento web, integração de APIs, modelagem de dados, arquitetura de software e implementação de algoritmos de recomendação.
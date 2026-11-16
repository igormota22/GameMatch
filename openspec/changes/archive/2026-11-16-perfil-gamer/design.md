# Design

## Context

A fundação e a change de identidade/contas já entregaram ASP.NET Core com
controllers, ASP.NET Core Identity, JWT, EF Core/Npgsql, ProblemDetails,
`/health` e o shell Angular 22 com rotas lazy, signals, Tailwind e interceptor
bearer. Os projetos `GameMatch.Application` e `GameMatch.Domain` ainda contêm
apenas o placeholder `Class1`; os controllers de conta usam `UserManager`
diretamente. Esta change introduz o primeiro caso de uso e repositório do
produto, servindo de padrão para as fases seguintes.

As decisões globais de arquitetura, camadas, modelo de preferências e contrato
HTTP estão em [`docs/tdd.md`](../../../docs/tdd.md) (DT-05, DT-06 e DT-17) e a
motivação está no [`proposal.md`](proposal.md). O comportamento esperado está
na [spec da capability](specs/perfil-gamer/spec.md).

## Goals / Non-Goals

**Goals:**

- Persistir o perfil gamer 1:1 com o usuário e as preferências normalizadas,
  respeitando a regra de dependência entre camadas.
- Entregar `GET /perfil` e `PUT /perfil` com leitura sem efeito colateral,
  substituição atômica e validação explícita.
- Tornar a completude do perfil observável no contrato para a interface e para
  a F3.
- Entregar a tela `/perfil` funcional, acessível e coberta por testes, com
  leitura das preferências atuais e edição sob demanda.
- Exibir um resumo do perfil na tela de início para usuários autenticados,
  reutilizando o contrato existente.

**Non-Goals:**

- Catálogo, sincronização do snapshot e sugestões de valores a partir dos dados
  da FreeToGame (F2).
- Cálculo de compatibilidade, endpoint e tela de recomendações (F3).
- Favoritos e avaliações (F4).
- Concorrência otimista, cache, rate limiting, perfil público e exclusão de
  conta.
- Adicionar um campo de nome de exibição à conta: nesta fase o nome exibido é
  derivado do e-mail da sessão.

## Decisions

### LD-01 — Perfil 1:1 usando o identificador do usuário como chave

`PerfilGamer` tem `UsuarioId` (string) como chave primária e FK para
`AspNetUsers`. `PreferenciaPerfil` tem chave composta
`(UsuarioId, Tipo, Valor)`, sem surrogate `PerfilId`.

**Por que:** o TDD (DT-06 e ER da §4.1) já define o perfil como 1:1 e a
preferência unicamente por usuário, tipo e valor (INV-01). Usar o próprio
`UsuarioId` evita uma coluna redundante e mantém a unicidade no banco.

**Alternativas descartadas:** surrogate `PerfilId` (mais joins sem benefício);
JSONB (descartado no TDD por ser opaco para consultas e filtros).

### LD-02 — Enums persistidos como inteiros e normalização de valores

`TipoPreferencia` usa `Genero = 1`, `Plataforma = 2`, `Estilo = 3` e
`Experiencia = 4`, persistido como inteiro. Cada valor textual é normalizado no
caso de uso com `Trim` e colapso de espaços internos, limitado a 100
caracteres; a deduplicação da entrada é feita sem diferenciar maiúsculas de
minúsculas, preservando a grafia da primeira ocorrência.

**Por que:** a F3 comparará valores do perfil com gêneros, plataformas e tags
do catálogo; normalizar agora reduz duplicatas e divergências por espaçamento.
Preservar a grafia mantém a interface legível sem exigir uma tabela de
sinônimos.

**Alternativas descartadas:** gravar tudo em minúsculas (perde a apresentação);
coluna normalizada paralela (complexidade desnecessária no volume do MVP);
validar contra uma lista fixa (o catálogo ainda não existe nesta fase).

### LD-03 — Caso de uso e repositório como padrão para as próximas fases

`ServicoPerfil` fica em `GameMatch.Application`, recebe `IPerfilRepositorio`
(interface também em `Application`) e devolve DTOs/resultado de validação.
`PerfilRepositorio` fica em `GameMatch.Infrastructure` e usa o
`GameMatchDbContext`. `PerfilController` apenas extrai o `UsuarioId` do claim
`sub`, chama o serviço e traduz o resultado para HTTP.

**Por que:** mantém a regra de dependência e permite testar validação e
orquestração sem HTTP nem banco. Os controllers de conta existentes continuam
como estão; esta é a primeira feature de produto e estabelece o padrão.

**Alternativas descartadas:** acessar `DbContext` no controller (viola a
arquitetura); MediatR/CQRS (peso desnecessário para o escopo acadêmico).

### LD-04 — Leitura sem perfil é 200 com estado vazio

`GET /perfil` de um usuário ainda sem perfil retorna `200` com as quatro listas
vazias, `completo = false` e `atualizadoEm = null`, sem criar registro.

**Por que:** a ausência de perfil é o estado inicial esperado do onboarding
(RF-06/RB-07) e a interface precisa exibi-lo como formulário vazio, não como
erro. A leitura permanece livre de efeitos colaterais.

**Alternativas descartadas:** `404` (obrigaria a interface a tratar ausência
como erro); criar o perfil na primeira leitura (efeito colateral e registro
vazio desnecessário).

### LD-05 — `PUT /perfil` substitui o conjunto e valida antes de gravar

O salvamento valida a entrada inteira antes de qualquer alteração, normaliza e
deduplica os valores, e em uma única transação cria o perfil ou substitui todas
as preferências. A data `AtualizadoEm` é atualizada. A política é
last-write-wins, sem token de concorrência.

**Por que:** o formulário envia o estado completo do perfil; substituir o
conjunto evita lógica de diff e atende à edição contínua (RB-05). Perfil é
individual, então escrita concorrente do mesmo usuário não é um cenário do MVP.

**Alternativas descartadas:** `PATCH` incremental (mais casos de erro sem
benefício); concorrência otimista com `xmin`/rowversion (revisar apenas se
surgir edição simultânea).

### LD-06 — Contrato de leitura com `completo` e `atualizadoEm`

`PerfilDto` contém `generos`, `plataformas`, `estilos`, `experiencias`,
`completo` e `atualizadoEm`. `PerfilSalvarDto` contém apenas as quatro listas.
O `completo` é calculado pela regra de domínio e não é aceito na escrita.

**Por que:** RF-06 precisa ser observável antes da F3 e a interface consegue
sinalizar o estado do perfil sem recalcular regra de negócio. É uma extensão
local do DTO proposto no TDD §5.1, sem quebra de contrato.

**Alternativas descartadas:** a interface derivar completude das listas
(duplica a regra); endpoint separado de completude (superfície extra sem uso).

### LD-07 — Tela em modo de leitura com edição sob demanda e texto livre

A tela `/perfil` abre em modo de leitura, mostrando o nome de exibição derivado
do e-mail da sessão (parte antes de `@`, com inicial maiúscula), as quatro listas
atuais e o indicador de completude. A ação “Editar perfil” revela o formulário
com quatro áreas de texto preenchidas (um valor por linha); gêneros e plataformas
exigem ao menos um item e estilos e experiências podem ficar vazios. O componente
divide, remove linhas vazias e espaços e envia as listas; a API renormaliza e
valida. Após salvar com sucesso, a tela volta ao modo de leitura com os valores
retornados pela API.

**Por que:** o modo de leitura torna o perfil consultável sem risco de edição
acidental e mantém a ação de edição explícita. A entrada por texto livre
continua necessária porque a F1 não tem catálogo nem snapshot para oferecer
opções válidas, e o PRD descreve que o usuário “informa” as preferências. Texto
livre mantém a fase independente da F2 e não inventa uma lista que divergiria
dos gêneros, plataformas e tags reais da FreeToGame. A F2/F3 pode trocar a
entrada por seleção a partir de `/catalogo/filtros` e das tags, preservando o
contrato.

**Alternativas descartadas:** manter o formulário sempre visível (menos claro
sobre o estado atual do perfil); abrir a edição em modal (complexidade de foco e
responsividade sem benefício); listas fixas provisórias (risco de não casarem
com o snapshot); aguardar a F2 (bloqueia um requisito P0); entrada por vírgula
(pior para valores com espaços, como “Massively Multiplayer”).

### LD-08 — Persistência em tabelas snake_case com migração única

As tabelas novas são `perfis_gamer` (`usuario_id`, `atualizado_em`) e
`perfis_preferencias` (`usuario_id`, `tipo`, `valor`), seguindo o padrão da
tabela `registros_sistema`. A FK usa `ON DELETE CASCADE` e a migração é
`PerfilGamer`. Não há seed nem backfill.

**Por que:** segue a convenção já adotada para tabelas próprias, mantém a
integridade ao excluir usuário e não altera tabelas existentes.

**Alternativas descartadas:** nomes PascalCase (reservados às tabelas do
Identity no projeto); duas migrações separadas (não há entrega intermediária).

### LD-09 — Verificação em três níveis

Testes unitários cobrem a regra de completude, a normalização e a
deduplicação; testes de integração cobrem 401, leitura vazia, criação, edição,
422 e isolamento por token usando a fixture existente com PostgreSQL; testes
Angular cobrem o serviço, os estados de leitura/edição do perfil e o resumo na
home. A verificação manual segue CA-06/CA-07.

**Por que:** atende ao TC-01 e ao DT-16 com o que já existe no repositório, sem
introduzir CI.

### LD-10 — Resumo do perfil na tela de início

A tela de início exibe, apenas para usuários autenticados, uma seção com o nome
de exibição derivado do e-mail (`SessaoService.usuario`), as preferências
retornadas por `PerfilService.obter()` e o indicador de completude. O nome de
exibição usa a parte antes de `@`, com a primeira letra maiúscula (ex.:
`igor@teste.com` → `Igor`), sem novo campo de conta. A seção mostra um resumo das
listas e um atalho para `/perfil`; em perfil vazio ou incompleto, orienta a
configuração. Uma falha de leitura é tratada apenas dentro da seção e não bloqueia
o restante da home.

**Por que:** reaproveita o contrato existente e o mesmo serviço, sem novo
endpoint nem duplicação da regra de completude. O nome derivado do e-mail evita
ampliar o cadastro nesta fase, mantendo a alteração restrita ao frontend.

**Alternativas descartadas:** buscar o perfil junto com a sessão (altera o
contrato de autenticação); adicionar um campo `nome` agora (exige migração,
contrato e formulário de conta); esconder a seção quando a leitura falha (perde
o atalho de recuperação).

## Risks / Trade-offs

- **Texto livre pode divergir dos rótulos do snapshot da FreeToGame** → a
  normalização reduz o problema e a F2/F3 deve oferecer seleção a partir do
  catálogo; registrar como evolução, não como bloqueio da F1.
- **Last-write-wins pode perder uma edição concorrente** → aceitável para
  perfil individual; revisar se houver edição simultânea.
- **422 é produzido pelo caso de uso, não pelo `[ApiController]`** → erros de
  binding/JSON continuam 400; os testes de integração devem distinguir os dois
  casos e o contrato documenta a validação de negócio como 422.
- **Deduplicação sem diferenciar maiúsculas pode unir valores distintos apenas
  por caixa** → desejável para gêneros e plataformas; se a F3 precisar de
  distinção, a regra é reavaliada com dados reais.
- **Áreas de texto permitem erros de digitação** → a seleção assistida chega
  com o catálogo; até lá, a interface mostra exemplos e a API valida tamanho e
  obrigatoriedade.
- **O nome derivado do e-mail é apenas uma identificação visual** → pode não
  corresponder ao nome real do jogador; não é usado em regras de negócio e deve
  ser substituído quando a conta ganhar um campo próprio.
- **Home e `/perfil` leem o perfil de forma independente** → podem ficar
  momentaneamente desatualizados até a próxima navegação; aceitável para um
  registro pequeno e individual.
- **A falha da leitura do perfil na home não pode bloquear a página** → a seção
  trata o erro isoladamente e mantém o restante do conteúdo e o status da API.
- **Sem paginação ou cache** → o perfil é um registro pequeno e não há
  recomendações nesta fase.

## Migration Plan

1. Gerar a migração `PerfilGamer` no projeto `GameMatch.Infrastructure` com o
   projeto de inicialização `GameMatch.Api`.
2. Aplicar com `dotnet ef database update` no ambiente local (PostgreSQL do
   compose); não há backfill.
3. Rollback: `dotnet ef database update <migração-anterior>` remove as duas
   tabelas novas; como não há dados pré-existentes do perfil, a perda é
   limitada aos perfis criados na fase.
4. No frontend, a entrega é a nova rota lazy e a seção de resumo na tela de
   início; não há variável de ambiente nova.
5. Atualizar `docs/ambiente.md` apenas se o procedimento de migração já não
   estiver coberto.

## Open Questions

- A substituição das áreas de texto por seleção assistida deve ocorrer na F2
  (catálogo) ou na F3 (tags e opções do Modo Descoberta)? Decisão adiável; o
  contrato de `PUT /perfil` não muda.
- Exibir `atualizadoEm` na tela é desejável; nesta revisão ele aparece no modo
  de leitura de `/perfil`, mas não no resumo da home.
- Quando a conta ganhar um campo de nome, o nome derivado do e-mail deve ser
  substituído pelo nome informado? Decisão futura; não altera o contrato de
  perfil.

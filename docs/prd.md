# PRD — GameMatch

| Campo | Valor |
|---|---|
| Produto | GameMatch — plataforma web de descoberta e recomendação de jogos gratuitos |
| Status | Rascunho |
| Data | 2026-11-12 |
| Origem do pedido | Prompt baseado no `README.md` do projeto: "crie um PRD com as regras de negócio propostas" |
| Revisão da entrevista | Não existe `docs/entrevista.md`; as ambiguidades foram resolvidas diretamente com o solicitante por sistema de questões (ver §13) |

---

## 1. Resumo executivo

O **GameMatch** é uma plataforma web que ajuda jogadores a descobrir jogos
gratuitos alinhados ao seu perfil. A proposta de valor vai além de uma lista de
jogos populares: cada sugestão traz uma **pontuação de compatibilidade** e uma
**explicação dos fatores** que justificam a recomendação.

O produto resolve o problema de excesso de opção no universo de jogos
gratuitos: o jogador sabe o que gosta, mas não tem uma forma simples de
filtrar o catálogo e entender *por que* um jogo combina com ele.

**Resultado pretendido:** o jogador cria uma conta, configura seu perfil gamer,
recebe recomendações personalizadas com explicação transparente e gerencia
favoritos e avaliações de forma contínua.

## 2. Contexto e problema

### Fatos observados (documentados no projeto)

- O catálogo de jogos gratuitos é grande e disperso; a FreeToGame API expõe
  gêneros, plataformas e desenvolvedores como dados brutos, sem
  personalização (fato de integração, conforme `README.md` e `docs/arquitetura.md`).
- A fundação do projeto já existe: API ASP.NET Core em quatro camadas,
  frontend Angular, PostgreSQL via Docker, autenticação com JWT e rotas de
  conta implementadas (`docs/arquitetura.md`, `README.md`).
- O projeto é acadêmico, com previsão de entrega de versão funcional até
  novembro de 2026 (`README.md`).

### Hipóteses (não validadas com usuários)

- Jogadores de jogos gratuitos abandonam a busca por não encontrar títulos
  compatíveis com gênero, plataforma e estilo desejados.
- A transparência da recomendação (mostrar o "porquê") aumenta a confiança
  na sugestão em relação a uma lista sem explicação.

### Oportunidade

Combinar dados externos (FreeToGame) com preferências próprias do usuário em
um algoritmo de recomendação explicável, entregue em um produto web simples.

## 3. Objetivos e métricas de sucesso

| # | Objetivo | Indicador | Definição de medição | Meta | Baseline |
|---|---|---|---|---|---|
| O-1 | Facilitar a descoberta de jogos gratuitos | % de usuários ativos que geram ao menos uma recomendação por semana | Usuários com ≥1 exibição de recomendação ÷ usuários ativos na semana | A definir | A definir (sem dados de uso) |
| O-2 | Tornar a recomendação confiável | % de recomendações com explicação completa exibida | Recomendações com todos os fatores visíveis ÷ total de recomendações | 100% (meta de produto: sempre exibir explicação) | 0 (produto em desenvolvimento) |
| O-3 | Engajar com favoritos e avaliações | Nº médio de favoritos e avaliações por usuário ativo | Contagens por usuário no mês | A definir | A definir |
| O-4 | Entregar o MVP na janela acadêmica | Itens do MVP concluídos e verificados | Checklist da §6.1 aprovado | 100% até novembro/2026 | Em desenvolvimento |

> Métricas de uso (O-1, O-3) dependem de telemetria ainda não definida; ficam
> como `A definir` com ação de validação (§13).

## 4. Público-alvo e stakeholders

### Perfis de usuário

| Perfil | Necessidade principal |
|---|---|
| Jogador casual | Encontrar jogos gratuitos rápidos de iniciar, compatíveis com gênero e plataforma que já gosta |
| Jogador explorador | Descobrir títulos de gêneros diferentes do habitual (Modo Descoberta) |
| Jogador ativo | Manter favoritos, avaliações e preferências sempre atualizados |

### Stakeholders

| Stakeholder | Interesse | Responsabilidade |
|---|---|---|
| Solicitante / equipe do projeto | Entrega acadêmica até novembro/2026 | Priorização e validação do PRD |
| FreeToGame.com | Atribuição da origem dos dados | Exigência de atribuição no rodapé (fato documentado em `docs/arquitetura.md`) |
| Usuários da plataforma | Recomendações úteis e dados privados | Uso da plataforma |
| Avaliadores acadêmicos | Demonstração dos conhecimentos aplicados | Revisão da entrega |

## 5. Escopo

### 5.1 MVP (Escopo inicial do produto)

Itens confirmados pelo solicitante como MVP:

1. Cadastro e autenticação de usuários.
2. Configuração do perfil gamer.
3. Integração com dados da FreeToGame (via snapshot local, ver §11).
4. Catálogo com pesquisa e filtros.
5. Sistema de recomendações personalizadas.
6. Pontuação e explicação de compatibilidade.
7. Gerenciamento de jogos favoritos.
8. Avaliação de jogos.
9. **Modo Descoberta** (confirmado pelo solicitante: entra no MVP).

### 5.2 Evoluções posteriores

- **Histórico** de jogos avaliados e explorados (citado no `README.md` como
  funcionalidade principal, mas fora do MVP — decisão do solicitante).
- Exclusão de conta (fora do MVP; ver §13).
- Perfil público com avaliações visíveis.
- Integração direta com a API FreeToGame em tempo real (hoje: snapshot local).

### 5.3 Fora de escopo

- Pagamentos, compras ou qualquer modelo de monetização.
- Jogos pagos (apenas jogos gratuitos).
- Redes sociais, comentários, seguidores ou recomendação entre usuários.
- Aplicativos nativos (apenas web).
- Notificações push ou e-mail de marketing.
- IA generativa de conteúdo.

### 5.4 Prioridades e dependências

| Prioridade | Itens | Dependências |
|---|---|---|
| P0 | Conta/autenticação, perfil gamer, catálogo sincronizado | Fundação já existente (API, banco, shell web) |
| P1 | Recomendações com pontuação e explicação, favoritos, avaliações | P0 (perfil e catálogo) |
| P1 | Modo Descoberta | P1 de recomendações |
| P2 | Histórico, exclusão de conta | Fora do MVP |

## 6. Jornadas e casos de uso

### J-1 — Cadastro e primeira sessão

1. Visitante acessa `/criar-conta`, informa login e senha.
2. Sistema cria a conta, autentica automaticamente e redireciona para o app.
3. **Alternativa:** já possui conta → `/entrar` com e-mail e senha.
4. **Erro:** credenciais inválidas → mensagem de erro sem criar sessão.

### J-2 — Configurar perfil gamer

1. Usuário autenticado acessa a configuração do perfil.
2. Informa **obrigatoriamente** gêneros favoritos e plataformas de interesse.
3. Opcionalmente informa estilos de gameplay, preferências de experiência e
   tempo de sessão típico.
4. Salva; as preferências passam a alimentar as recomendações.
5. **Ausência de dados:** sem preferências obrigatórias preenchidas, as
   recomendações ficam **indisponíveis**, com aviso pedindo a configuração do
   perfil (decisão do solicitante).

### J-3 — Explorar o catálogo e pesquisar

1. Usuário abre o catálogo e pesquisa por nome ou filtra por gênero,
   plataforma e demais características disponíveis.
2. Resultados exibem card com dados do jogo (imagem, gênero, plataforma,
   desenvolvedor) e atribuição FreeToGame.
3. **Erro/ausência de dados:** nenhum resultado → estado vazio com sugestão
   de limpar filtros.

### J-4 — Receber recomendações personalizadas

1. Com perfil preenchido, o usuário abre a seção de recomendações.
2. Para cada jogo, o sistema exibe: pontuação de compatibilidade e explicação
   dos fatores que contribuíram (gêneros, plataformas, estilos, experiência).
3. **Sem perfil preenchido:** recomendações indisponíveis (J-2.4).
4. **Sem catálogo sincronizado:** estado vazio indicando indisponibilidade.

### J-5 — Modo Descoberta

1. Usuário ativa o Modo Descoberta e escolhe uma das **opções predefinidas**
   de experiência desejada no momento (lista de opções: conteúdo a definir,
   ver §13-Q3).
2. O sistema retorna sugestões compatíveis com a experiência escolhida,
   combinada com o perfil do usuário, também com explicação.
3. **Alternativa:** voltar às recomendações padrão do perfil a qualquer momento.

### J-6 — Favoritar um jogo

1. Usuário favorita um jogo a partir do card ou da detalhação.
2. O jogo aparece na lista de favoritos; favoritar de novo não cria duplicado.
3. Usuário pode remover dos favoritos a qualquer momento.

### J-7 — Avaliar um jogo

1. Usuário atribui **1 a 5 estrelas** a um jogo (uma avaliação por jogo).
2. Pode alterar ou remover a avaliação depois.
3. **Erro:** tentar avaliar o mesmo jogo duas vezes sem alterar → prevalece a
   avaliação existente (edição).

### J-8 — Gerenciar conta

1. Usuário acessa `/conta`, vê dados da conta, altera senha (exige senha
   atual) ou encerra a sessão.
2. Sessão persiste após recarregar a página (token em `localStorage`).
3. Visitante em rota protegida → redirecionado para `/entrar?destino=...`.

## 7. Requisitos funcionais

### Conta e sessão

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-01 | O sistema deve permitir cadastro com e-mail/login e senha, com login automático após criação | P0 | RB-01 |
| RF-02 | O sistema deve autenticar por e-mail e senha e emitir token de sessão | P0 | RB-02 |
| RF-03 | O sistema deve exibir dados da conta autenticada e permitir alteração de senha exigindo a senha atual | P0 | RB-03 |
| RF-04 | O sistema deve manter a sessão persistente entre recargas da página e proteger rotas autenticadas | P0 | RB-04 |

### Perfil gamer

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-05 | O sistema deve permitir cadastrar e editar o perfil gamer com gêneros favoritos (obrigatório), plataformas (obrigatório), estilos de gameplay, preferências de experiência e tempo de sessão típico (opcionais) | P0 | RB-05, RB-06 |
| RF-06 | O sistema deve exigir as preferências mínimas (gêneros e plataformas) para disponibilizar recomendações | P1 | RB-07; depende de RF-05 |

### Catálogo, pesquisa e filtros

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-07 | O sistema deve exibir catálogo de jogos gratuitos a partir dos dados sincronizados da FreeToGame | P0 | RB-08, RB-12; depende de sincronização (§11) |
| RF-08 | O sistema deve permitir busca por nome e filtros por gênero, plataforma e demais características disponíveis nos dados | P0 | RB-09 |
| RF-09 | O sistema deve exibir estado vazio quando a busca/filtro não retornar resultados | P0 | — |

### Recomendações e compatibilidade

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-10 | O sistema deve calcular uma pontuação de compatibilidade (0–100) entre o perfil do usuário e cada jogo, considerando gêneros favoritos, plataformas de interesse, estilos de gameplay e preferências de experiência | P1 | RB-07, RB-10; fórmula e pesos: A definir (§13-Q1) |
| RF-11 | O sistema deve exibir, para cada recomendação, a explicação dos fatores que contribuíram para a pontuação | P1 | RB-11 |
| RF-12 | O sistema deve ordenar recomendações por pontuação de compatibilidade | P1 | RB-10 |
| RF-13 | O sistema deve exibir aviso e ocultar recomendações enquanto as preferências mínimas não forem preenchidas | P1 | RB-07 |

### Modo Descoberta

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-14 | O sistema deve oferecer o Modo Descoberta com opções predefinidas de experiência desejada, selecionáveis pelo usuário no momento da consulta | P1 | RB-13; lista de opções: A definir (§13-Q3) |
| RF-15 | O sistema deve combinar a experiência escolhida com o perfil do usuário para gerar sugestões com explicação | P1 | RB-11, RB-13; depende de RF-10, RF-14 |

### Favoritos e avaliações

| ID | Requisito | Prioridade | Regra de negócio / dependência |
|---|---|---|---|
| RF-16 | O sistema deve permitir favoritar e remover jogos, sem duplicados e sem limite de quantidade | P1 | RB-14 |
| RF-17 | O sistema deve permitir avaliar um jogo de 1 a 5 estrelas, limitado a uma avaliação por jogo, editável e removível | P1 | RB-15 |
| RF-18 | O sistema deve listar os favoritos do usuário | P1 | RB-14 |

## 8. Regras de negócio

| ID | Regra |
|---|---|
| RB-01 | O cadastro cria uma conta única por e-mail/login; o login automático ocorre apenas na criação da conta |
| RB-02 | A autenticação é feita por e-mail + senha; a sessão é representada por token (JWT) enviado como `Authorization: Bearer` |
| RB-03 | A senha só pode ser alterada pelo próprio usuário autenticado informando a senha atual |
| RB-04 | Rotas autenticadas exigem sessão válida; visitantes são redirecionados para `/entrar` com parâmetro de destino |
| RB-05 | O perfil gamer pertence a um único usuário e pode ser editado a qualquer momento; alterações passam a valer nas próximas recomendações |
| RB-06 | Campos obrigatórios do perfil: gêneros favoritos e plataformas. Opcionais: estilos de gameplay, preferências de experiência, tempo de sessão típico |
| RB-07 | Recomendações só ficam disponíveis com as preferências mínimas (RB-06) preenchidas; sem elas, exibe-se aviso de configuração do perfil |
| RB-08 | O catálogo é composto por jogos gratuitos obtidos da FreeToGame (via snapshot local, §11), com atribuição da fonte exibida na interface |
| RB-09 | Pesquisa e filtros operam sobre os campos disponíveis no catálogo sincronizado (nome, gênero, plataforma, etc.); combinam-se entre si |
| RB-10 | A pontuação de compatibilidade varia de 0 a 100 e considera quatro fatores: gêneros favoritos, plataformas de interesse, estilos de gameplay e preferências de experiência; **pesos e fórmula exata: A definir** (§13-Q1) |
| RB-11 | Toda recomendação exibe os fatores que contribuíram para a pontuação, em linguagem compreensível ao usuário |
| RB-12 | Dados de jogos não são criados pelos usuários; alterações no catálogo vêm apenas da sincronização da fonte |
| RB-13 | O Modo Descoberta usa opções predefinidas de experiência selecionadas pelo usuário e cruza-as com o perfil do usuário; **conteúdo da lista de opções: A definir** (§13-Q3) |
| RB-14 | Um jogo pode ser favoritado uma única vez por usuário; remover o favorito não afeta avaliações; não há limite de favoritos |
| RB-15 | Cada usuário avalia um jogo no máximo uma vez, com nota de 1 a 5 estrelas; a avaliação pode ser alterada ou removida |

## 9. Requisitos não funcionais

| ID | Requisito | Observação |
|---|---|---|
| RNF-01 | **Segurança:** senhas armazenadas com hash adequado (ASP.NET Core Identity); token JWT assinado com chave de no mínimo 32 caracteres, não versionada, fornecida por segredo/user-secrets/variável de ambiente; API deve encerrar com mensagem clara se a chave faltar | Baseado em fatos já documentados no `README.md` |
| RNF-02 | **Privacidade:** perfil, favoritos, avaliações e (futuro) histórico são dados privados do usuário; não há perfil público no escopo atual | Decisão do solicitante |
| RNF-03 | **Atribuição de dados:** exibir atribuição à FreeToGame.com com link ativo no rodapé | Exigência dos termos da API, documentada em `docs/arquitetura.md` |
| RNF-04 | **Erros de API:** respostas de erro em formato ProblemDetails (RFC 7807) com `traceId` | Decisão técnica já adotada na fundação |
| RNF-05 | **Disponibilidade da interface:** com a API indisponível, o shell web carrega e sinaliza o status sem quebrar a navegação | Comportamento já previsto no `README.md` |
| RNF-06 | **Desempanho:** tempo de resposta das recomendações e do catálogo: A definir (§13-Q5) | Sem baseline medido |
| RNF-07 | **Acessibilidade:** contraste adequado, navegação por teclado e labels acessíveis nos fluxos principais; nível exato de conformidade: A definir | Sem auditoria ainda |
| RNF-08 | **Testes:** suítes de backend (xUnit) e frontend (Angular CLI) executáveis e verdes antes de cada entrega | Prática já estabelecida na fundação |
| RNF-09 | **Observabilidade:** endpoint `/health` com verificação do banco; demais métricas/logs estruturados: A definir | Sem baseline |

## 10. Critérios de aceite

### Conta e sessão (RF-01 a RF-04)

- **CA-01** (RF-01): Dado um novo e-mail e senha válidos, quando o usuário
  cria a conta, então ele é autenticado automaticamente e acessa o app.
- **CA-02** (RF-01): Dado um e-mail já cadastrado, quando o usuário tenta
  criar a conta, então o sistema informa o conflito e nenhuma conta duplicada
  é criada.
- **CA-03** (RF-02): Dadas credenciais corretas, quando o usuário faz login,
  então a sessão é iniciada; dadas credenciais incorretas, então o login é
  negado com mensagem de erro e sem sessão.
- **CA-04** (RF-03): Dado um usuário autenticado, quando ele altera a senha
  informando a senha atual correta, então a nova senha passa a valer; com
  senha atual incorreta, a alteração é rejeitada.
- **CA-05** (RF-04): Dada uma sessão válida, quando a página é recarregada,
  então o usuário continua autenticado; sem sessão ao acessar `/conta`, então
  é redirecionado para `/entrar?destino=/conta`.

### Perfil gamer (RF-05, RF-06)

- **CA-06** (RF-05): Dado um usuário autenticado, quando ele salva o perfil
  sem gêneros ou sem plataformas, então a validação impede o salvamento;
  preenchendo ambos, o perfil é salvo e pode ser editado depois.
- **CA-07** (RF-05): Dado um perfil com apenas campos opcionais ausentes,
  quando ele é salvo, então aceita-se o salvamento (opcionais podem ficar vazios).

### Catálogo (RF-07 a RF-09)

- **CA-08** (RF-07): Dado um catálogo sincronizado, quando o usuário abre o
  catálogo, então os jogos são exibidos com dados de origem e atribuição
  FreeToGame visível; sem sincronização, então exibe-se estado vazio.
- **CA-09** (RF-08): Dado um termo de busca ou filtro, quando aplicado, então
  apenas jogos correspondentes são exibidos; combinando busca + filtro, então
  o resultado satisfaz ambas as condições.
- **CA-10** (RF-09): Dada uma busca sem resultados, então é exibido estado
  vazio com opção de limpar a busca/filtros.

### Recomendações (RF-10 a RF-13)

- **CA-11** (RF-10, RF-12): Dado um usuário com preferências mínimas e um
  catálogo sincronizado, quando as recomendações são geradas, então cada item
  exibe pontuação entre 0 e 100 e a lista está ordenada de maior para menor.
- **CA-12** (RF-11): Dada uma recomendação, quando exibida, então os fatores
  que contribuíram (gênero, plataforma, estilo, experiência) estão visíveis.
- **CA-13** (RF-13): Dado um usuário sem preferências mínimas, quando ele
  acessa a seção de recomendações, então as recomendações ficam indisponíveis
  com aviso para configurar o perfil; após preencher o perfil, então ficam
  disponíveis.

### Modo Descoberta (RF-14, RF-15)

- **CA-14** (RF-14): Dado um usuário, quando ele ativa o Modo Descoberta,
  então as opções predefinidas de experiência são apresentadas para escolha.
- **CA-15** (RF-15): Dada uma opção de experiência escolhida, quando o
  resultado é gerado, então as sugestões combinam a experiência com o perfil e
  exibem explicação; sem preferências mínimas, então aplica-se CA-13.

### Favoritos e avaliações (RF-16 a RF-18)

- **CA-16** (RF-16, RF-18): Dado um jogo, quando o usuário o favorita, então
  ele aparece na lista de favoritos; favoritar de novo não duplica; remover
  o faz sumir da lista sem afetar avaliações.
- **CA-17** (RF-17): Dado um jogo, quando o usuário avalia com nota válida
  (1–5), então a avaliação fica registrada; tentar nova avaliação substitui a
  anterior; nota fora de 1–5 é rejeitada; o usuário pode remover a avaliação.

### Não funcionais

- **CA-18** (RNF-01): Dada a ausência da chave JWT, quando a API inicia, então
  ela encerra com mensagem de erro em português; com chave válida, então
  assina e valida tokens normalmente.
- **CA-19** (RNF-03): Dada qualquer página pública ou autenticada, então o
  rodapé exibe atribuição à FreeToGame.com com link ativo.
- **CA-20** (RNF-08): Dado o repositório em estado de entrega, quando `dotnet
  test api/GameMatch.sln` e `npm test -- --watch=false` são executados, então
  todos os testes passam.

## 11. Dados e integrações

### Entidades principais

| Entidade | Informações essenciais | Ciclo de vida |
|---|---|---|
| Usuário / Conta | e-mail/login, senha (hash), dados de conta | Criada no cadastro; alteração de senha; **sem exclusão no MVP** |
| Perfil gamer | gêneros favoritos*, plataformas*, estilos de gameplay, preferências de experiência, tempo de sessão típico | Criado após cadastro; editável; alimenta recomendações |
| Jogo | dados da FreeToGame (id externo, título, gêneros, plataformas, desenvolvedor, thumbnail, descrição, atribuição) | Somente criado/atualizado pela sincronização |
| Favorito | usuário + jogo | Criado/removido pelo usuário; único por par |
| Avaliação | usuário + jogo + nota (1–5) | Única por par; editável/removível |
| Recomendação | usuário + jogo + pontuação + fatores | Calculada sob demanda (não necessariamente persistida — decisão de design no TDD) |
| (Futuro) Histórico | usuário + jogos avaliados/explorados | Fora do MVP |

\* obrigatórias para disponibilizar recomendações (RB-06, RB-07).

### Integração FreeToGame

- **Fonte:** FreeToGame API — dados de jogos gratuitos (título, gêneros,
  plataformas, desenvolvedor, mídia).
- **Modo no MVP:** sincronização para banco PostgreSQL a partir de **snapshot
  local** em `seed/`, permitindo demonstração offline e reproduzível
  (decisão documentada em `docs/arquitetura.md`; a integração em tempo real
  ainda não existe).
- **Restrição conhecida:** atribuição obrigatória à FreeToGame.com (RNF-03).
- **Desconhecido:** frequência de atualização do snapshot, volume exato de
  jogos e campos disponíveis em cada jogo — a validar na fase de catálogo.

### Fronteiras externas

| Interface | Tipo | Estado |
|---|---|---|
| API interna (Angular ↔ ASP.NET Core) | HTTP/JSON, prefixo `/api` via proxy em desenvolvimento | Conta implementada; demais endpoints a construir |
| Banco PostgreSQL | EF Core + Npgsql com migrations | Fundação pronta |
| FreeToGame | API externa (dados) | Snapshot local; chamada direta: fora do MVP |

## 12. Riscos, premissas e dependências

| # | Item | Tipo | Impacto | Mitigação / validação |
|---|---|---|---|---|
| R-1 | Fórmula da pontuação de compatibilidade ainda não definida | Risco | Recomendações pouco relevantes ou difíceis de explicar | Definir pesos e validação no TDD e testar com casos reais (§13-Q1) |
| R-2 | Snapshot da FreeToGame pode estar desatualizado ou incompleto | Risco | Catálogo pobre afeta recomendações | Validar conteúdo do snapshot em `seed/` na fase de catálogo |
| R-3 | Sem dados de uso, metas de engajamento são estimativas | Risco | Métricas O-1/O-3 sem baseline | Instrumentar medição mínima; revisar metas após primeiro uso (§13-Q4) |
| R-4 | Janela acadêmica até novembro/2026 | Risco | Escopo não entregue no prazo | Prioridades da §5.4; cortar evoluções (§5.2) antes do MVP |
| R-5 | Exigência de perfil mínimo pode frustrar novos usuários | Risco | Abandono antes das primeiras recomendações | Fluxo de onboarding guiado para preencher preferências mínimas |
| P-1 | Fundação técnica já existe (API, banco, contas, shell web) | Premissa | — | Conforme `docs/arquitetura.md` |
| P-2 | Projeto é acadêmico, sem requisito de produção | Premissa | Escopo e metas orientados à demonstração | Manter fora de escopo itens de produção (§5.3) |
| D-1 | Snapshot da FreeToGame | Dependência | Catálogo indisponível sem ele | Fase de catálogo (fase 3 da evolução) |
| D-2 | Chave JWT configurada no ambiente | Dependência | API não inicia sem ela | RNF-01 e procedimento no `README.md` |

## 13. Questões em aberto e próximos passos

| # | Questão | Status | Responsável sugerido | Forma de validação |
|---|---|---|---|---|
| Q1 | Pesos exatos e fórmula da pontuação de compatibilidade (0–100) | A definir — decidido: manter como pendente | Engenharia + produto | Definir no TDD de recomendações; testar com cenários de exemplo |
| Q2 | Faixas de exibição da pontuação (ex.: barra, cores, rótulos) | A definir | Design + produto | Revisão de UI no protótipo |
| Q3 | Conteúdo exato da lista de opções predefinidas do Modo Descoberta | A definir — decidido: serão opções predefinidas, não texto livre | Produto | Workshop de ideias + revisão no protótipo |
| Q4 | Metas numéricas de engajamento (O-1, O-3) e telemetria | A definir | Produto | Definir evento mínimo e coletar baseline na primeira utilização |
| Q5 | Tempos de resposta esperados (RNF-06) | A definir | Engenharia | Medir no ambiente local e definir orçamento |
| Q6 | Exclusão de conta e retratação de dados | Fora do MVP (decisão do solicitante) | Produto | Reavaliar pós-MVP |
| Q7 | Histórico de jogos avaliados/explorados | Evolução pós-MVP (decisão do solicitante) | Produto | Incluir em change posterior |
| Q8 | Frequência e automação da sincronização do snapshot | A definir | Engenharia | Definir na fase de catálogo |

### Próximos passos

1. Revisão e aprovação deste PRD por produto/engenharia.
2. Gerar o TDD com a definição técnica de Q1 (fórmula de compatibilidade),
   Q3 (opções do Modo Descoberta) e contratos de dados (§11).
3. Priorizar changes OpenSpec seguindo a ordem da §5.4.

---

*Documento gerado pela skill `generate-prd` a partir do `README.md` e
`docs/arquitetura.md`, com decisões coletadas por sistema de questões junto ao
solicitante. Nenhum código foi implementado.*

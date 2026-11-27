# Design

## Context

Ver `proposal.md` — Why. O frontend é Angular 22 zoneless com standalone + signals, Tailwind v4 e rotas lazy (DT-04). A fundação visual da locadora já existe em `web/src/styles.css` e foi aplicada ao shell, à página inicial, à autenticação, à conta, ao perfil, ao catálogo, às recomendações e aos favoritos. A página 404 ainda usa a paleta `emerald/slate` e um botão com utilitários avulsos.

A página inicial (`Inicio`) carrega `/api/health` e, para autenticados, `/api/perfil`; a vitrine é hoje um conjunto de três capas decorativas com `aria-hidden`. A criação de conta (`CriarConta`) autentica automaticamente e redireciona para `/conta`. O `FavoritosService` já expõe `listar(pagina, tamanho)` sobre `GET /api/favoritos`.

Restrições relevantes: a rota `**` é pública, tem título `GameMatch — Página não encontrada` e carregamento lazy; o componente `NaoEncontrado` não tem estado nem dependências. `inicio.spec.ts` usa `HttpTestingController` e verifica `/api/health` e `/api/perfil`, então a nova chamada de favoritos precisa ser refletida nos testes. Ver `specs/identidade-visual/spec.md` para o contrato.

## Goals / Non-Goals

**Goals:**

- Aplicar a identidade da locadora à página 404 com uma composição simples, legível e coerente com o restante do site.
- Trocar as capas genéricas da vitrine por até três favoritos reais, com estados de visitante, vazio, carregamento e erro.
- Redirecionar o cadastro para `/perfil`.
- Reutilizar o design system e os serviços existentes, sem dependências novas.
- Preservar textos exigidos, rota, título, shell, acessibilidade e responsividade.

**Non-Goals:**

- Alterar `app.routes.ts`, contratos da API, serviços de domínio ou guards.
- Criar novos componentes, classes CSS ou animações elaboradas.
- Reformular outras páginas ou persistir favoritos adicionais.

## Decisions

1. **Ficha de atendimento na 404** — a página usa `ficha-locacao` com `cabecalho-ficha` ("Atendimento" / "Balcão 01"), um `carimbo` com `Erro 404`, o `h1` `Página não encontrada`, a orientação e o CTA `Voltar para o início` em `botao-formulario botao-formulario--primario`. *Alternativa descartada*: painel escuro centralizado; repetiria a estética anterior. *Por que*: a ficha remete ao balcão da locadora e mantém a hierarquia `h1` → texto → link.

2. **Selo de "fita extraviada"** — `selo-locadora` decorativo (`aria-hidden`) ao lado do título, reforçando a identidade sem competir com a mensagem.

3. **Zero CSS novo** — todas as classes necessárias já existem e foram validadas nas etapas anteriores. *Por que*: menor risco de regressão e consistência automática com os tokens e o `prefers-reduced-motion` global.

4. **Vitrine orientada a dados** — `Inicio` injeta `FavoritosService` e, quando autenticado, chama `listar(1, 3)` uma vez no `ngOnInit`. O estado é exposto em signals (`favoritosVitrine`, `carregandoFavoritos`, `erroFavoritos`). *Alternativa descartada*: usar apenas `ids` e buscar cada jogo no catálogo; exigiria chamadas extras e não traria a capa/título prontos. *Por que*: o endpoint de favoritos já devolve os cards paginados, e o limite de 3 é aplicado no servidor.

5. **Estados explícitos da vitrine** — a vitrine mantém a moldura de prateleira e troca o conteúdo conforme o estado:
   - autenticado com favoritos: até 3 cards com capa, `alt`, título e link para `/favoritos`;
   - autenticado sem favoritos: `Você ainda não tem jogos favoritos.` com link `Explorar catálogo` para `/catalogo`;
   - visitante: `Entre para ver seus favoritos.` com link `Entrar` para `/entrar`;
   - carregando: `Carregando favoritos...` em `role="status"`;
   - falha: aviso em português com link para `/favoritos`, sem derrubar saúde nem perfil.
   O wrapper deixa de ser `aria-hidden` porque passa a conter conteúdo real. *Alternativa descartada*: manter as capas genéricas para visitantes; o solicitante pediu a substituição e a mensagem com CTA de entrada.

6. **Redirecionamento do cadastro** — `CriarConta` passa a chamar `router.navigateByUrl('/perfil')` após a sessão ser iniciada. Não há `destino` no fluxo de cadastro; a autenticação, as validações e as mensagens permanecem. *Alternativa descartada*: reaproveitar o `destino` do login; o cadastro não recebe esse parâmetro e o pedido é ir direto ao perfil. *Por que*: o perfil é onde o usuário configura as preferências que habilitam recomendações (J-2).

7. **Sem mudanças de comportamento fora do escopo** — o template da 404 mantém `h1`, parágrafo e `routerLink="/"`; a home mantém status da API, resumo do perfil e CTAs; o cadastro mantém o restante do fluxo. *Por que*: RF-19 exige apenas apresentação, e os ajustes de comportamento são os dois solicitados.

## Risks / Trade-offs

- [Texto pequeno sobre superfícies coloridas] → usar os pares já verificados (`tinta/papel`, `papel/marinho`, `vermelho/papel`) e limitar o amarelo a acentos.
- [Selo decorativo sendo anunciado por leitores de tela] → marcar o selo e a ficha decorativa como `aria-hidden="true"`.
- [Rolagem horizontal em 390px] → composição em coluna com `flex-wrap`, `min-w-0` e verificação em 390px/1440px.
- [Testes da home quebram pela nova chamada `/api/favoritos`] → atualizar `inicio.spec.ts` para responder à chamada em todos os cenários autenticados e cobrir os novos estados; `http.verify()` continua garantindo que nenhuma requisição fique pendente.
- [Falha de favoritos derrubando a home] → isolar o erro em `erroFavoritos` e manter saúde/perfil independentes; a vitrine mostra aviso com link para `/favoritos`.
- [Regressão na rota 404 ou no cadastro] → não alterar `app.routes.ts`; validar URL inexistente e cadastro completo no navegador.

## Migration Plan

Branch `feature/identidade-visual-pagina-nao-encontrada` a partir da `main`: (1) template da 404 e teste, (2) vitrine de favoritos e testes da home, (3) redirecionamento do cadastro e teste, (4) `npm test -- --watch=false` e `npm run build`, (5) verificação manual em 390px/1440px, (6) PR. Rollback = revert da PR antes do merge; não há migrations, contratos ou dependências.

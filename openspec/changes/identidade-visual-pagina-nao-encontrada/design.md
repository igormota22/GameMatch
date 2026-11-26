# Design

## Context

Ver `proposal.md` — Why. O frontend é Angular 22 zoneless com standalone + signals, Tailwind v4 e rotas lazy (DT-04). A fundação visual da locadora já existe em `web/src/styles.css` e foi aplicada ao shell, à página inicial, à autenticação, à conta, ao perfil, ao catálogo, às recomendações e aos favoritos. A página 404 ainda usa a paleta `emerald/slate` e um botão com utilitários avulsos.

Restrições relevantes: a rota `**` é pública, tem título `GameMatch — Página não encontrada` e carregamento lazy em `app.routes.ts`; o componente `NaoEncontrado` não tem estado nem dependências. Não há teste dedicado à página. A reformulação deve preservar os textos `Erro 404`, `Página não encontrada`, `O endereço acessado não existe ou foi movido.`, `Voltar para o início` e o link para `/`. Ver `specs/identidade-visual/spec.md` para o contrato.

## Goals / Non-Goals

**Goals:**

- Aplicar a identidade da locadora à página 404 com uma composição simples, legível e coerente com o restante do site.
- Reutilizar o design system existente, sem classes novas e sem dependências.
- Preservar textos, rota, título, shell, acessibilidade e responsividade.

**Non-Goals:**

- Alterar `app.routes.ts`, `nao-encontrado.ts`, serviços, guards ou contratos.
- Criar novos componentes, classes CSS ou animações elaboradas.
- Reformular outras páginas.

## Decisions

1. **Ficha de atendimento como metáfora** — a página usa `ficha-locacao` com `cabecalho-ficha` ("Atendimento" / "Balcão 01"), um `carimbo` com `Erro 404`, o `h1` `Página não encontrada`, a orientação e o CTA `Voltar para o início` em `botao-formulario botao-formulario--primario`. *Alternativa descartada*: painel escuro centralizado; repetiria a estética anterior e não dialogaria com o restante do site. *Por que*: a ficha remete ao balcão da locadora e mantém a hierarquia `h1` → texto → link.

2. **Selo de "fita extraviada" como elemento gráfico** — `selo-locadora` decorativo (`aria-hidden`) ao lado do título, reforçando a identidade sem competir com a mensagem. *Alternativa descartada*: ilustração ou SVG novo; adicionaria peso e manutenção sem ganho funcional.

3. **Zero CSS novo** — todas as classes necessárias (`ficha-locacao`, `cabecalho-ficha`, `carimbo`, `selo-locadora`, `etiqueta-promo`, `botao-formulario`, `link-formulario`) já existem e foram validadas nas etapas anteriores. *Por que*: menor risco de regressão e consistência automática com os tokens e o `prefers-reduced-motion` global.

4. **Sem mudanças de comportamento** — o template mantém o `h1`, o parágrafo e o `routerLink="/"`; nenhum arquivo `.ts` ou teste é alterado. *Por que*: RF-19 exige apenas apresentação e a rota já está correta.

## Risks / Trade-offs

- [Texto pequeno sobre superfícies coloridas] → usar os pares já verificados (`tinta/papel`, `papel/marinho`, `vermelho/papel`) e limitar o amarelo a acentos.
- [Selo decorativo sendo anunciado por leitores de tela] → marcar o selo e a fita como `aria-hidden="true"`.
- [Rolagem horizontal em 390px] → composição em coluna com `flex-wrap`, `min-w-0` e verificação em 390px/1440px.
- [Regressão na rota 404] → não alterar `app.routes.ts`; validar uma URL inexistente no navegador.

## Migration Plan

Branch `feature/identidade-visual-pagina-nao-encontrada` a partir da `main`: (1) reformular o template, (2) rodar `npm test -- --watch=false` e `npm run build`, (3) verificação manual em 390px/1440px, (4) abrir o PR. Rollback = revert da PR antes do merge; não há migrations, contratos ou dependências.

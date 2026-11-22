# Design

## Context

O frontend (DT-04) é Angular 22 zoneless com standalone + signals, Tailwind v4 via `@tailwindcss/postcss` e rotas lazy por feature. O estilo global é apenas `web/src/styles.css` com `@import "tailwindcss";` (sem `@theme`), `web/src/app/app.css` está vazio e todo o restante são classes utilitárias inline nos templates (`bg-slate-950`, `emerald-500` etc.). Não há camada de componentes de UI compartilhada além de `layout/cabecalho` e `layout/rodape`.

Restrições observadas na inspeção: os testes (`*.spec.ts`) afirmam **textos visíveis, papéis ARIA e estrutura** (ex.: rótulos do menu, "Dados fornecidos por", "Configurar perfil"), não classes CSS — mas qualquer mudança de copy visível quebra suíte. A atribuição FreeToGame no rodapé é exigida por CA-19. Ver `proposal.md` para a motivação e `specs/identidade-visual/spec.md` para o contrato de comportamento.

## Goals / Non-Goals

**Goals:**
- Estabelecer a base visual reutilizável (tokens, tipografia, texturas, elementos gráficos) que as próximas changes de páginas vão consumir.
- Reformular shell (cabeçalho/rodapé) e página inicial com a identidade da locadora, mantendo textos, rotas e comportamentos exigidos por CA e testes.
- Manter `npm test -- --watch=false` verde e a interface responsiva/legível.

**Non-Goals:**
- Reformular catálogo, recomendações, perfil, favoritos, conta e autenticação (changes futuras, após validação da identidade).
- Alterar arquitetura, serviços, rotas, guards, contratos de API ou regras de negócio.
- Adicionar dependências npm novas ou bibliotecas de componentes.

## Decisions

1. **Tokens no Tailwind v4 via `@theme` em `styles.css`** — cores da paleta (azul-marinho, azul elétrico, amarelo, laranja/vermelho, papel, cinza/preto), famílias tipográficas, raios, sombras e texturas (SVG/CSS embutido) viram tokens semânticos (`--color-tinta`, `--color-papel` etc.). *Alternativa descartada*: arquivo CSS por componente ou Sass — quebraria a convenção utilitária atual e exigiria tooling novo (DT-04 mantido).

2. **Fontes auto-hospedadas** — pares de fontes livres (ex.: display condensada de revista + corpo legível) como `woff2` em `web/src/public/fontes/`, referenciadas no `styles.css`. *Alternativa descartada*: CDN do Google Fonts (falha offline/privacidade) e fontes de sistema (identidade fraca).

3. **Paleta em dois níveis** — núcleo (papel/envelhecido + azul-marinho) para superfícies, azul elétrico e amarelo para estrutura/CTAs, laranja/vermelho só em selos promocionais; contraste AA verificado par-a-par. Neon roxo/azul explicitamente proibido. *Por que*: evita o "todas as cores ao mesmo tempo" e garante escopo consistente nas próximas páginas.

4. **Aplicação gradual sem retrocompatibilidade de estilos** — esta change aplica os tokens a `app.html` (shell), cabeçalho, rodapé e `features/inicio`; as demais features mantêm a paleta slate/emerald até as changes delas. *Risco aceito*: transição visual mista por pouco tempo — aceitável porque as páginas antigas continuam funcionais e coesas em si.

5. **Elementos gráficos como CSS + marcadores HTML mínimos** — etiquetas/selos/carimbos via classes utilitárias novas e pseudo-elementos; só criariamos componente compartilhado se a marcação se repetir em ≥3 lugares. *Alternativa descartada*: biblioteca de componentes própria — escopo maior que o necessário.

6. **Interações só em CSS** — hover/focus com `@media (hover: hover)` e `prefers-reduced-motion`, sem JS; conteúdo essencial nunca depende de hover. *Por que*: barato, testável manualmente, sem tocar em componentes TS.

7. **Preservação de copy testada** — os textos afirmados em specs (menu, CTAs, atribuição, estados) não mudam de conteúdo; se um ajuste editorial for necessário, o teste é atualizado na mesma commit com justificativa.

8. **Documentos globais atualizados na mesma PR** — novo RF-19 (+ CA-23) em `docs/prd.md` e nova DT-19 em `docs/tdd.md` registrando o design system sobre DT-04, conforme `AGENTS.md`.

## Risks / Trade-offs

- [Quebra de testes por mudança de copy/DOM] → rodar `npm test -- --watch=false` por etapa; preservar textos e papéis ARIA; ajustar teste junto quando editório inevitável.
- [Regressão de contraste (RNF-07)] → validar pares de cor AA (texto/fundo) antes de aprovar cada etapa; preferir papel escuro sobre amarelo para texto pequeno.
- [Escopo vazar para outras páginas] → tarefas limitadas a shell + `features/inicio`; demais features intocadas nesta change.
- [Peso de fontes/texturas] → `woff2` subset PT-BR; texturas em SVG/CSS (sem bitmaps pesados).
- [Identidade reprovada na validação] → trabalho de tokens é reutilizável; trocar paleta/tipografia é ajuste nos tokens, não refazer páginas.

## Migration Plan

Branch `feature/identidade-visual-locadora` a partir da `main`, em etapas revisáveis: (1) tokens + fontes, (2) shell, (3) página inicial, (4) verificação. Cada etapa: `npm test -- --watch=false` + revisão visual manual (J-1/J-3 no navegador, desktop e móvel). Rollback = revert da PR antes do merge; após merge, nova PR de ajuste (sem migrations de banco envolvidas — mudança só de apresentação).

## Open Questions

Nenhuma que afete specs, abordagem ou tarefas. A escolha específica das fontes livres pode ser ajustada na validação sem mudar o contrato (decisão 2 define apenas o mecanismo).

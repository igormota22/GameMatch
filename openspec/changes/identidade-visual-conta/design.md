# Design

## Context

Ver `proposal.md` — Why. O frontend é Angular 22 zoneless com standalone + signals, Tailwind v4 e rotas lazy (DT-04). A fundação visual da locadora já existe em `web/src/styles.css` (tokens `@theme`, texturas, `superficie-papel`, `borda-embalagem`, `ficha-locacao`, `etiqueta-promo`, `selo-locadora`, `carimbo`) e já foi aplicada ao shell e à página inicial. As três telas desta change ainda usam a paleta antiga (slate/emerald) e repetem longas listas de utilitários nos formulários.

Restrições relevantes: os testes afirmam textos visíveis e estrutura, então a remoção do item `Minha conta` do cabeçalho exige atualizar `cabecalho.spec.ts` na mesma entrega; a página de perfil precisa de `RouterLink` (mudança de TS apenas em `perfil.ts`) e o teste dela precisa de `provideRouter`. Ver `specs/identidade-visual/spec.md` para o contrato de comportamento.

## Goals / Non-Goals

**Goals:**

- Aplicar a identidade da locadora em `/entrar`, `/criar-conta` e `/conta` com uma linguagem de balcão de atendimento e ficha de locação.
- Centralizar os estilos de formulário em classes globais reutilizáveis, reduzindo a repetição de utilitários e garantindo foco, erro e contraste consistentes.
- Mover o acesso a `/conta` para dentro de `/perfil`, removendo o atalho próprio do cabeçalho.
- Manter a suíte verde e a interface acessível/responsiva.

**Non-Goals:**

- Alterar serviços, guards, rotas, contratos da API ou regras de validação.
- Reformular catálogo, recomendações, favoritos ou página 404.
- Criar biblioteca de componentes ou adicionar dependências npm.
- Alterar textos visíveis exigidos por critérios de aceite.

## Decisions

1. **Classes de formulário no CSS global** — criar em `styles.css`, dentro de `@layer components`, um conjunto enxuto: `ficha-formulario`, `cabecalho-ficha`, `campo-formulario`, `rotulo-formulario`, `entrada-formulario`, `dica-formulario`, `erro-formulario`, `aviso-formulario` (com variantes de erro/sucesso), `botao-formulario` (primário/secundário) e `link-formulario`. Usam apenas os tokens existentes. *Alternativa descartada*: repetir utilitários Tailwind nas três telas (já é a origem da inconsistência) ou criar componentes de formulário (escopo maior que o necessário). *Por que*: as três telas compartilham a mesma necessidade e a mudança fica restrita a CSS + templates.

2. **Composição de balcão/ficha** — `/entrar` e `/criar-conta` usam uma ficha de papel com faixa superior em azul-marinho (tipografia display), carimbo/selo discreto e o formulário sobre a ficha; `/conta` usa duas fichas (dados da conta e alteração de senha) com o mesmo cabeçalho. A metáfora é decorativa: a estrutura semântica (`h1`, `h2`, `form`, `label`, `dl`) e a ordem dos campos não mudam. *Alternativa descartada*: layout de duas colunas com ilustração — pouco espaço em 390px e sem ganho funcional.

3. **Acesso à conta dentro do perfil** — remover o `<li>` de `Minha conta` do cabeçalho e adicionar em `/perfil` uma ação visível `Minha conta` com `routerLink="/conta"`, no cabeçalho da página (junto ao título). Em `perfil.ts`, adicionar apenas `RouterLink` aos imports do componente. *Alternativa descartada*: esconder o link em menu suspenso — pior descoberta e acessibilidade. *Por que*: mantém a rota e o guard intactos, reduz a densidade da navbar e concentra as ações da conta no perfil.

4. **Preservação de copy e semântica** — todos os textos atuais permanecem (títulos, rótulos, dicas, mensagens de validação, botões e links). Erros de validação continuam dentro do `label` e recebem `role="alert"` para anúncio por tecnologia assistiva. *Por que*: atende RNF-07/CA-23 sem reescrever contratos testados.

5. **Interações discretas e movimento seguro** — hover/active apenas em CSS, com `motion-safe`/`motion-reduce` onde houver transformação decorativa; a regra global de `prefers-reduced-motion` já reduz transições. Conteúdo e ações nunca dependem de hover. *Alternativa descartada*: animações em JS — custo sem benefício.

6. **Testes ajustados na mesma entrega** — `cabecalho.spec.ts` passa a afirmar a ausência de `Minha conta` no estado autenticado; `perfil.spec.ts` ganha `provideRouter([])` e uma verificação do link `Minha conta` apontando para `/conta`. *Por que*: a mudança de navegação é intencional e precisa de cobertura explícita, não de um teste silenciosamente removido.

## Risks / Trade-offs

- [Testes quebram pela remoção do item de menu] → atualizar `cabecalho.spec.ts` no mesmo commit e rodar `npm test -- --watch=false` por etapa.
- [`RouterLink` no perfil exige provider no teste] → adicionar `provideRouter([])` em `perfil.spec.ts` antes de rodar a suíte.
- [Contraste insuficiente em etiquetas amarelas] → texto pequeno sempre em `tinta` sobre `papel`/`amarelo`; vermelho apenas como acento e mensagens com `vermelho` sobre `papel` (5,52:1).
- [Fundo pautado da ficha atrapalhar a leitura dos campos] → usar papel com textura, sem linhas de escrita, atrás dos inputs.
- [Escopo vazar para outras páginas] → tarefas limitadas às três telas, ao cabeçalho e ao perfil.
- [Regressão funcional silenciosa] → não alterar `.ts` de login/cadastro/conta; validar manualmente os fluxos J-1 e J-8.

## Migration Plan

Branch `feature/identidade-visual-conta` a partir da `main`, em etapas revisáveis: (1) classes de formulário no CSS, (2) `/entrar` e `/criar-conta`, (3) `/conta`, (4) navegação (cabeçalho + perfil), (5) verificação. Cada etapa roda `npm test -- --watch=false`; ao final, `npm run build` e revisão manual em 390px/1440px. Rollback = revert da PR antes do merge; não há migrations de banco nem mudanças de contrato.

## Open Questions

Nenhuma que afete specs, abordagem ou tarefas. Ajustes finos de composição visual podem ser feitos na validação sem alterar o contrato.

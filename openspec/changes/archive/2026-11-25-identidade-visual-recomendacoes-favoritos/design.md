# Design

## Context

Ver `proposal.md` — Why. O frontend é Angular 22 zoneless com standalone + signals, Tailwind v4 e rotas lazy (DT-04). A fundação visual da locadora já existe em `web/src/styles.css` (tokens `@theme`, texturas, `superficie-papel`, `borda-embalagem`, `ficha-locacao`, `ficha-formulario`, `cabecalho-ficha`, `entrada-formulario`, `botao-formulario`, `aviso-formulario`, `etiqueta-promo`, `etiqueta-colada`, `selo-locadora`, `carimbo`, `prateleira`, `cartao-jogo`, `etiqueta-caracteristica`) e já foi aplicada ao shell, à página inicial, à autenticação, à conta, ao perfil e ao catálogo. `/recomendacoes` e `/favoritos` ainda usam a paleta slate/emerald.

Restrições relevantes: os testes afirmam textos visíveis, papéis ARIA, atributos (`aria-live`, `aria-pressed`, `aria-label`, `role`), o seletor `select#experiencia`, a ausência de `<li>` no estado de perfil incompleto, e que os botões `Tentar novamente`, `Anterior`, `Próxima` e `Remover avaliação` tenham exatamente esses textos. A reformulação deve preservar tudo isso. Ver `specs/identidade-visual/spec.md` para o contrato de comportamento.

## Goals / Non-Goals

**Goals:**

- Aplicar a identidade da locadora em `/recomendacoes` (vitrine + Modo Descoberta + explicação por fatores) e `/favoritos` (ficha de locação + estrelas + paginação).
- Adicionar apenas as classes compartilhadas necessárias (fator de recomendação, selo de pontuação e botão de estrela), reutilizando o restante do design system.
- Preservar copy, estados, acessibilidade e responsividade.
- Manter a suíte verde sem alterar comportamento testado.

**Non-Goals:**

- Alterar o cálculo de recomendação, serviços, guards, rotas ou contratos da API.
- Reformular catálogo, perfil, conta ou o shell.
- Criar biblioteca de componentes ou adicionar dependências npm.
- Alterar textos exigidos por critérios de aceite ou por testes existentes.

## Decisions

1. **Reutilizar o design system antes de criar classes** — os cards usam `cartao-jogo`; gêneros usam `etiqueta-caracteristica`; painéis e paginação usam `superficie-papel`; erros usam `aviso-formulario`; o Modo Descoberta usa `ficha-formulario`/`cabecalho-ficha` com `entrada-formulario`; o cabeçalho usa `etiqueta-promo` e `selo-locadora`. *Alternativa descartada*: novo conjunto de componentes de página; duplicaria o que já existe. *Por que*: consistência com o catálogo e o perfil recém-reformulados.

2. **Poucas classes novas, focadas no que se repete** — adicionar em `styles.css` (`@layer components`): `selo-pontuacao` (chip de porcentagem sobre a capa), `fator-recomendacao` (linha de fator com rótulo, contribuição e correspondências) e `botao-estrela` (botão de ícone de estrela com foco visível e estado `aria-pressed="true"`). *Alternativa descartada*: repetir dezenas de utilitários em cada card/fator/estrela; é a origem da inconsistência atual. *Por que*: os três padrões aparecem repetidamente e ganham estados de foco/hover consistentes.

3. **Recomendações: vitrine de balcão** — cabeçalho com etiqueta promocional e título display; painel de Modo Descoberta em ficha com `cabecalho-ficha`; cards `cartao-jogo` com capa, selo de pontuação, título, desenvolvedor, gêneros, bloco `Por que este jogo?` com descrição e fatores em `fator-recomendacao`, e botão de favorito no rodapé. *Alternativa descartada*: manter cards escuros e trocar só as cores; não transmitiria a metáfora física nem resolveria o contraste. *Por que*: a semântica (`h1`, `h2`, `h3`, `h4`, `ul`, `li`, `button`) permanece intacta.

4. **Favoritos: ficha de locação** — cabeçalho de coleção com contagem anunciada; cards `cartao-jogo` com capa, título, desenvolvedor, gêneros, bloco `Sua avaliação` com estrelas em `botao-estrela`, ação `Remover avaliação` e ação `Remover dos favoritos`; paginação em faixa de papel. O estado vazio vira painel de papel com CTA `Explorar catálogo`. *Alternativa descartada*: lista horizontal; a grade atual funciona bem em 390px e 1440px e preserva a leitura. *Por que*: reforça a ideia de ficha/coleção sem mudar a estrutura de navegação.

5. **Contraste e legibilidade primeiro** — texto pequeno em `tinta`/`tinta-suave` sobre `papel` ou `papel-escuro`; amarelo apenas como fundo com texto `tinta` ou como acento; vermelho para erros e para a ação de remoção; nunca texto pequeno em `papel` sobre `amarelo`. *Por que*: atende RNF-07/CA-23 e corrige o baixo contraste atual das duas telas.

6. **Interações discretas e movimento seguro** — elevação de cards apenas com `motion-safe`; a regra global de `prefers-reduced-motion` já reduz transições. Nenhum conteúdo ou ação depende de hover; o estado das estrelas é indicado por `aria-pressed` e por preenchimento do SVG, não só por cor.

7. **Sem mudanças de comportamento** — os templates mantêm os mesmos controles, ids, textos, `data-*` (quando existirem) e atributos ARIA; os arquivos `.ts` e os testes não devem mudar. Se algum teste quebrar por estrutura inevitável, ele é ajustado preservando a asserção de comportamento, nunca removendo cobertura. *Por que*: RF-19 exige alteração apenas de apresentação.

## Risks / Trade-offs

- [Testes quebram por mudança de estrutura/atributos] → manter textos exatos, `select#experiencia`, `aria-label` das estrelas e do favorito, e não renderizar `<li>` no estado de perfil incompleto; rodar `npm test -- --watch=false` após cada etapa.
- [Botões de estrela com texto quebrando o helper `botao(texto)`] → manter as estrelas apenas com SVG (`aria-hidden`) e `aria-label`; nenhum texto visível dentro do botão.
- [Contraste insuficiente em chips e selos] → usar os pares já verificados (`tinta/amarelo`, `papel/marinho`, `tinta/papel`, `vermelho/papel`) e evitar texto pequeno sobre laranja.
- [Cards com muitos fatores estourando a largura em 390px] → `flex-wrap`, `min-w-0`, `break-words` e teste visual em 390px.
- [Escopo vazar para outras páginas] → tarefas limitadas a `/recomendacoes`, `/favoritos` e classes compartilhadas.
- [Regressão funcional silenciosa] → não alterar `.ts`; validar manualmente Modo Descoberta, favoritar, avaliar, remover e paginar.

## Migration Plan

Branch `feature/identidade-visual-recomendacoes-favoritos` a partir da `main`, em etapas revisáveis: (1) classes compartilhadas no CSS, (2) `/recomendacoes`, (3) `/favoritos`, (4) verificação. Cada etapa roda `npm test -- --watch=false`; ao final, `npm run build` e revisão manual em 390px/1440px. Rollback = revert da PR antes do merge; não há migrations de banco nem mudanças de contrato.

## Open Questions

Nenhuma que afete specs, abordagem ou tarefas. Ajustes finos de composição visual podem ser feitos na validação sem alterar o contrato.

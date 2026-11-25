# Design

## Context

Ver `proposal.md` — Why. O frontend é Angular 22 zoneless com standalone + signals, Tailwind v4 e rotas lazy (DT-04). A fundação visual da locadora já existe em `web/src/styles.css` (tokens `@theme`, texturas, `superficie-papel`, `borda-embalagem`, `ficha-locacao`, `ficha-formulario`, `cabecalho-ficha`, `entrada-formulario`, `botao-formulario`, `aviso-formulario`, `etiqueta-promo`, `etiqueta-colada`, `selo-locadora`, `carimbo`, `prateleira`) e já foi aplicada ao shell, à página inicial, à autenticação e à conta. `/perfil` e `/catalogo` ainda usam a paleta slate/emerald.

Restrições relevantes: os testes afirmam textos visíveis, papéis ARIA, atributos (`aria-live`, `aria-pressed`, `aria-labelledby`, `aria-describedby`, `role`), seletores por id (`#busca`, `#genero`, `#plataforma`, `#tag`), atributos `data-campo`/`data-valor` das opções e o estado dos controles. A reformulação deve preservar tudo isso. Ver `specs/identidade-visual/spec.md` para o contrato de comportamento.

## Goals / Non-Goals

**Goals:**

- Aplicar a identidade da locadora em `/perfil` (leitura e edição) e `/catalogo` com metáforas de ficha do locatário, balcão de busca e prateleira de jogos.
- Adicionar apenas as classes compartilhadas necessárias (card de jogo, etiqueta de característica e opção de preferência), reutilizando o restante do design system.
- Preservar copy, validações, estados, acessibilidade e responsividade.
- Manter a suíte verde sem alterar comportamento testado.

**Non-Goals:**

- Alterar serviços, guards, rotas, contratos da API ou regras de negócio.
- Reformular recomendações, favoritos, página 404 ou o shell.
- Criar biblioteca de componentes ou adicionar dependências npm.
- Alterar textos exigidos por critérios de aceite ou por testes existentes.

## Decisions

1. **Reutilizar o design system antes de criar classes** — `/perfil` usa `superficie-papel` + `fita-adesiva` no modo de leitura e `ficha-formulario` + `cabecalho-ficha` no modo de edição; `/catalogo` usa `ficha-formulario` para busca/filtros e `borda-embalagem`/`prateleira` para a listagem. *Alternativa descartada*: criar um conjunto novo de componentes de página; seria escopo maior e duplicaria o que já existe. *Por que*: mantém consistência com o shell e as telas anteriores.

2. **Poucas classes novas, focadas no que se repete** — adicionar em `styles.css` (`@layer components`): `cartao-jogo` (superfície de caixa com borda, sombra e elevação segura em hover), `etiqueta-caracteristica` (chip de gênero em amarelo/tinta) e `etiqueta-caracteristica--plataforma` (variante em papel/marinho), além de `opcao-preferencia` (linha de checkbox com área de clique, hover e foco). *Alternativa descartada*: repetir dezenas de utilitários em cada card/opção; já é a origem da inconsistência atual. *Por que*: os três padrões aparecem muitas vezes nas duas telas e ganham estados de foco/hover consistentes.

3. **Perfil: ficha do locatário** — no modo de leitura, o bloco principal vira uma ficha de papel com cabeçalho de identificação do jogador, selo de completude (`etiqueta-promo` azul para completo, vermelha para incompleto), preferências em `dl` e ações (`Editar perfil`, `Minha conta`). No modo de edição, a ficha contém o `fieldset` com as opções do catálogo, o grupo de legados destacado em laranja/vermelho e as ações `Cancelar`/`Salvar perfil`. *Alternativa descartada*: manter cards escuros e trocar só as cores; não transmitiria a metáfora física nem resolveria o contraste. *Por que*: a semântica (`h1`, `dl`, `fieldset`, `legend`, `label`) permanece intacta.

4. **Catálogo: balcão + prateleira** — cabeçalho com etiqueta promocional e título display; busca/filtros em ficha com `cabecalho-ficha`; resultados em grade de `cartao-jogo` com capa em destaque, etiquetas de gênero/plataforma e botão de favorito no rodapé do card; paginação em faixa de papel; estados vazios/erro em `aviso-formulario` ou painel de papel. *Alternativa descartada*: cards horizontais ou lista; a grade atual funciona bem em 390px e 1440px e preserva a leitura. *Por que*: reforça a prateleira da locadora sem mudar a estrutura de navegação.

5. **Contraste e legibilidade primeiro** — texto pequeno em `tinta`/`tinta-suave` sobre `papel` ou `papel-escuro`; amarelo apenas como fundo com texto `tinta` ou como acento; vermelho para erros e selos; nunca texto pequeno em `papel` sobre `amarelo`. *Por que*: corrige o baixo contraste atual do perfil (texto claro sobre fundo de papel) e atende RNF-07/CA-23.

6. **Interações discretas e movimento seguro** — elevação de cards e deslocamento de botões apenas com `motion-safe`; a regra global de `prefers-reduced-motion` já reduz transições. Nenhum conteúdo ou ação depende de hover. *Alternativa descartada*: animações em JS — custo sem benefício.

7. **Sem mudanças de comportamento** — os templates mantêm os mesmos controles, ids, nomes de formulário, `data-*`, textos e atributos ARIA; os arquivos `.ts` e os testes não devem mudar. Se algum teste quebrar por estrutura inevitável, ele é ajustado preservando a asserção de comportamento, nunca removendo cobertura. *Por que*: RF-19 exige alteração apenas de apresentação.

## Risks / Trade-offs

- [Testes quebram por mudança de estrutura/atributos] → manter ids, `data-*`, textos e ARIA; rodar `npm test -- --watch=false` após cada etapa.
- [Contraste insuficiente em chips e selos] → usar os pares já verificados (`tinta/amarelo`, `papel/marinho`, `tinta/papel`, `vermelho/papel`) e evitar texto pequeno sobre laranja.
- [Cards com muitas etiquetas estourando a largura em 390px] → `flex-wrap`, `min-w-0`, `break-words` e teste visual em 390px.
- [Fundo texturizado atrapalhar a leitura dos filtros] → usar papel com textura apenas como superfície e campos claros (`entrada-formulario`).
- [Escopo vazar para outras páginas] → tarefas limitadas a `/perfil`, `/catalogo` e classes compartilhadas.
- [Regressão funcional silenciosa] → não alterar `.ts`; validar manualmente leitura/edição do perfil, busca/filtros, paginação, favoritos e estados vazios.

## Migration Plan

Branch `feature/identidade-visual-perfil-catalogo` a partir da `main`, em etapas revisáveis: (1) classes compartilhadas no CSS, (2) `/perfil`, (3) `/catalogo`, (4) verificação. Cada etapa roda `npm test -- --watch=false`; ao final, `npm run build` e revisão manual em 390px/1440px. Rollback = revert da PR antes do merge; não há migrations de banco nem mudanças de contrato.

## Open Questions

Nenhuma que afete specs, abordagem ou tarefas. Ajustes finos de composição visual podem ser feitos na validação sem alterar o contrato.

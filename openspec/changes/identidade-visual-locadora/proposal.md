# Proposal

## Why

O MVP do GameMatch está funcionalmente concluído (RF-01 a RF-18, fases F1–F5 do TDD, suíte web com 81 testes verdes), mas a interface é um dashboard genérico em estilo SaaS (fundo slate escuro, acentos emerald, cards arredondados sem personalidade). Para o TCC, o site precisa ter identidade própria: uma locadora de videogames dos anos 90 traduzida em experiência digital — sem alterar a funcionalidade já validada.

## What Changes

- Criar o **design system da locadora** no frontend: tokens de cor (azul-marinho, azul elétrico, amarelo, laranja/vermelho promocional, branco envelhecido/papel, cinza/preto de material físico), tipografia editorial de revistas/embalagens, texturas sutis de papel/plástico, bordas e sombras inspiradas em caixas de PS1/PS2/N64/SNES e etiquetas de preço — via Tailwind v4 (`@theme`) e CSS global, sem substituir a pilha de estilo existente (DT-04).
- Reformular o **shell do site** (cabeçalho e rodapé) com a linguagem da locadora, preservando rótulos de navegação, rotas, autenticação e a atribuição FreeToGame no rodapé (RNF-03, CA-19).
- Reformular a **página inicial** como fachada/vitrine da locadora: identidade marcante, destaques e chamadas promocionais, mantendo o conteúdo funcional já existente (resumo do perfil, status da API, CTAs) e os textos exigidos por testes e critérios de aceite.
- Adicionar **elementos gráficos reutilizáveis** (etiquetas, selos/adesivos, fichas de locação) e **interações físicas** (hover de retirar caixa da prateleira, etiquetas ao passar o mouse, carimbos), respeitando `prefers-reduced-motion` e não prejudicando navegação por teclado.
- **Preservar integralmente**: arquitetura, contratos e endpoints da API, serviços, guards, regras de negócio, dados reais, funcionalidades e responsividade. Mudança estritamente visual/presentacional.
- Atualizar os documentos globais vivos na mesma PR: **`docs/prd.md`** (novo requisito de identidade visual — ex. RF-19 — com critério de aceite correspondente, rastreando aos RNF-03/05/07 existentes) e **`docs/tdd.md`** (nova DT registrando a decisão do design system sobre DT-04).
- Esta change cobre apenas a **fundação visual + página inicial + shell**; catálogo, recomendações, perfil, favoritos, conta e autenticação serão reformulados em changes separadas após validação da identidade.

## Capabilities

### New Capabilities

- `identidade-visual`: identidade visual da locadora dos anos 90 aplicada ao design system global (tokens, tipografia, texturas, componentes gráficos), ao shell do site (cabeçalho/rodapé) e à página inicial, com restrições de acessibilidade, responsividade e preservação da funcionalidade existente.

### Modified Capabilities

<!-- Nenhuma capacidade existente muda de requisito: catálogo, perfil-gamer,
     recomendações e favoritos-avaliacoes mantêm comportamento idêntico; apenas
     serão repintados visualmente em changes futuras. -->

## Impact

- **Código afetado (web)**: `web/src/styles.css` (tokens/texturas globais), `web/src/index.html` (fontes), `web/src/app/layout/cabecalho/*`, `web/src/app/layout/rodape/*`, `web/src/app/features/inicio/*`, novos assets em `web/src/public/` e possivelmente novos componentes compartilhados de UI. Testes de componentes afetados (`cabecalho.spec.ts`, `inicio.spec.ts`) podem precisar de ajuste apenas se textos visíveis mudarem — os textos exigidos por CA/testes serão preservados.
- **Não afetado**: `api/` (contratos, endpoints, DTOs, modelos), serviços e guards de `web/src/app/core`, rotas (`app.routes.ts`), demais features.
- **Documentos**: `docs/prd.md` e `docs/tdd.md` atualizados na mesma PR (requisito e decisão da identidade visual).
- **Dependências**: nenhuma nova (sem novos pacotes npm; assets próprios).
- **Rastreabilidade**: existentes RNF-03 (atribuição), RNF-05 (disponibilidade da interface), RNF-07 (acessibilidade), CA-19; novo RF/CA de identidade visual a ser registrado no PRD durante a implementação.

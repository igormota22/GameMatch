# Proposal

## Why

A identidade visual da locadora já cobre a fundação, o shell, a página inicial, a autenticação, a conta, o perfil, o catálogo, as recomendações e os favoritos. A página de rota inexistente (`**` → `/nao-encontrado`) é a única que ainda usa a paleta genérica anterior (`emerald/slate`) e um botão fora do design system. Concluir o RF-19 nessa tela fecha a experiência em todas as páginas — inclusive quando o visitante erra o endereço ou segue um link antigo — sem alterar rotas, título da página ou o comportamento de retorno ao início.

## What Changes

- Reformular visualmente a página **404** (`web/src/app/features/nao-encontrado/nao-encontrado.html`) com a linguagem da locadora: ficha de atendimento, carimbo de erro, selo de "fita extraviada", orientação e CTA de retorno ao início.
- Reutilizar os tokens e classes existentes (`ficha-locacao`, `cabecalho-ficha`, `carimbo`, `selo-locadora`, `etiqueta-promo`, `superficie-papel`, `botao-formulario`, `link-formulario`), sem criar classes novas nem dependências.
- Preservar integralmente os **textos visíveis** (`Erro 404`, `Página não encontrada`, `O endereço acessado não existe ou foi movido.`, `Voltar para o início`), o `h1`, o link para `/`, a rota wildcard `**`, o título `GameMatch — Página não encontrada` e a ausência de guard.
- Manter **acessibilidade e responsividade**: foco visível, contraste AA, layout sem rolagem horizontal em 390px e 1440px e respeito a `prefers-reduced-motion`.
- **Não** alterar `docs/prd.md` nem `docs/tdd.md`: RF-19, CA-23 e DT-19 já registram a identidade visual entregue por etapas; esta change é a etapa final. Backend, shell, demais páginas e contratos permanecem fora do escopo.

## Capabilities

### New Capabilities

<!-- Nenhuma capacidade nova: a change conclui a capability de identidade visual já existente. -->

### Modified Capabilities

- `identidade-visual`: adiciona o requisito da página 404 com a identidade da locadora, preservando textos, link de retorno, título da rota e acessibilidade sob a identidade visual.

## Impact

- **Código afetado (web)**: `web/src/app/features/nao-encontrado/nao-encontrado.html`. Nenhuma alteração prevista em `web/src/styles.css` — as classes necessárias já existem; se a composição exigir um ajuste pontual, ele é restrito a uma classe compartilhada reutilizável.
- **Não afetado**: `web/src/app/app.routes.ts` (rota `**`, título e lazy loading), `nao-encontrado.ts`, serviços, guards, cabeçalho, rodapé, demais features, `api/` e contratos.
- **Documentos**: PRD e TDD reutilizados sem alteração (RF-19/CA-23/DT-19 já cobrem a entrega por etapas); a justificativa será registrada no PR.
- **Dependências**: nenhuma nova (sem pacotes npm adicionais).
- **Rastreabilidade**: RF-19, CA-23 e DT-19 (identidade visual); CA-19/RNF-03 (shell e atribuição preservados); RNF-07 (acessibilidade e responsividade); RNF-08/CA-20 (suíte verde).
- **Testes**: `npm test -- --watch=false` deve permanecer verde. A página não possui teste dedicado hoje; a verificação será manual (rota inexistente em 390px/1440px, foco por teclado, contraste, `prefers-reduced-motion` e link de retorno). Se a change adicionar um teste de componente, ele apenas fixa os textos e o link existentes, sem alterar comportamento.

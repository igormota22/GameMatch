# Proposal

## Why

A identidade visual da locadora já cobre a fundação, o shell, a página inicial, a autenticação, a conta, o perfil, o catálogo, as recomendações e os favoritos. A página de rota inexistente (`**` → `/nao-encontrado`) é a única que ainda usa a paleta genérica anterior (`emerald/slate`) e um botão fora do design system. Concluir o RF-19 nessa tela fecha a experiência em todas as páginas — inclusive quando o visitante erra o endereço ou segue um link antigo — sem alterar rotas, título da página ou o comportamento de retorno ao início.

Antes do arquivamento, o solicitante pediu dois ajustes leves de comportamento: a vitrine da página inicial deve mostrar até 3 jogos favoritos do usuário (em vez de capas genéricas), com aviso adequado quando não houver favoritos; e o cadastro deve redirecionar direto para `/perfil`, onde o usuário configura as preferências que alimentam as recomendações.

## What Changes

- Reformular visualmente a página **404** (`web/src/app/features/nao-encontrado/nao-encontrado.html`) com a linguagem da locadora: ficha de atendimento, carimbo de erro, selo de "fita extraviada", orientação e CTA de retorno ao início.
- Reutilizar os tokens e classes existentes (`ficha-locacao`, `cabecalho-ficha`, `carimbo`, `selo-locadora`, `etiqueta-promo`, `superficie-papel`, `botao-formulario`, `link-formulario`), sem criar classes novas nem dependências.
- Preservar integralmente os **textos visíveis** da 404 (`Erro 404`, `Página não encontrada`, `O endereço acessado não existe ou foi movido.`, `Voltar para o início`), o `h1`, o link para `/`, a rota wildcard `**`, o título `GameMatch — Página não encontrada` e a ausência de guard.
- Substituir as capas decorativas da **vitrine da página inicial** por até **3 jogos favoritos** do usuário autenticado (`GET /api/favoritos?pagina=1&tamanho=3`), exibindo capa, título e link para `/favoritos`. Para visitantes, exibir a mensagem `Entre para ver seus favoritos.` com link para `/entrar`; para autenticados sem favoritos, `Você ainda não tem jogos favoritos.` com link para `/catalogo`; em falha, um aviso com link para `/favoritos`, sem comprometer o status da API nem o resumo do perfil.
- Alterar o **redirecionamento após o cadastro** de `/conta` para `/perfil`, mantendo a autenticação automática, as validações, as mensagens e o restante do fluxo de criação de conta.
- Manter **acessibilidade e responsividade**: foco visível, contraste AA, layout sem rolagem horizontal em 390px e 1440px, imagens com `alt` e respeito a `prefers-reduced-motion`.
- **Não** alterar `docs/prd.md` nem `docs/tdd.md`: RF-19, CA-23 e DT-19 já registram a identidade visual entregue por etapas; o CA-01 e a jornada J-1 não fixam `/conta` como destino do cadastro, e o diagrama de perfil do TDD já assume o usuário sem preferências após o cadastro. Backend, shell, demais páginas e contratos permanecem fora do escopo.

## Capabilities

### New Capabilities

<!-- Nenhuma capacidade nova: a change conclui a capability de identidade visual já existente. -->

### Modified Capabilities

- `identidade-visual`: adiciona o requisito da página 404 com a identidade da locadora, o requisito da vitrine de favoritos na página inicial e a modificação do requisito de cadastro para redirecionar a `/perfil`, preservando textos, links, acessibilidade e o restante do comportamento existente.

## Impact

- **Código afetado (web)**: `web/src/app/features/nao-encontrado/nao-encontrado.html` (e novo `nao-encontrado.spec.ts`), `web/src/app/features/inicio/inicio.ts`, `inicio.html` e `inicio.spec.ts`, e `web/src/app/features/criar-conta/criar-conta.ts` (e novo `criar-conta.spec.ts`). Nenhuma alteração prevista em `web/src/styles.css` — as classes necessárias já existem; se a composição exigir um ajuste pontual, ele é restrito a uma classe compartilhada reutilizável.
- **Não afetado**: `web/src/app/app.routes.ts` (rota `**`, título e lazy loading), serviços e guards de `web/src/app/core`, contratos da API, cabeçalho, rodapé, demais features e `api/`. O `FavoritosService` é reutilizado sem alteração.
- **Documentos**: PRD e TDD reutilizados sem alteração (RF-19/CA-23/DT-19 cobrem a identidade visual por etapas; CA-01/J-1 não fixam o destino pós-cadastro); a justificativa será registrada no PR.
- **Dependências**: nenhuma nova (sem pacotes npm adicionais).
- **Rastreabilidade**: RF-19, CA-23 e DT-19 (identidade visual); RF-01, CA-01 e J-1 (cadastro e primeira sessão); RF-16/CA-16 (favoritos exibidos na vitrine); CA-19/RNF-03 (shell e atribuição preservados); RNF-07 (acessibilidade e responsividade); RNF-08/CA-20 (suíte verde).
- **Testes**: `npm test -- --watch=false` deve permanecer verde. A página 404 ganha teste dedicado; `inicio.spec.ts` passa a cobrir favoritos, visitante, vazio e erro (ajustando a expectativa da chamada `/api/favoritos`); `criar-conta.spec.ts` cobre o redirecionamento para `/perfil`. A verificação manual cobre 404, vitrine e cadastro em 390px/1440px, foco por teclado, contraste e `prefers-reduced-motion`.

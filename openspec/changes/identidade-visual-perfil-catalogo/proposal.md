# Proposal

## Why

A identidade visual da locadora já cobre a fundação, o shell, a página inicial, a autenticação e a conta. As páginas `/perfil` e `/catalogo`, porém, continuam com a paleta genérica anterior (slate/emerald), justamente nos dois fluxos centrais do produto: configurar o perfil gamer (J-2) e explorar o catálogo (J-3). Continuar o RF-19 nessas telas fecha a experiência de uso diário do jogador com a mesma linguagem visual, sem alterar os comportamentos já validados pelos RF-04 a RF-09 e seus critérios de aceite.

## What Changes

- Reformular visualmente **`/perfil`** com a linguagem da locadora dos anos 90 (paleta, tipografia, materiais, etiquetas/selos e metáfora de ficha do locatário), tanto no modo de leitura quanto no modo de edição.
- Reformular visualmente **`/catalogo`** com a metáfora de balcão/prateleira: cabeçalho de vitrine, ficha de busca e filtros, cards de jogo com capa, etiquetas de gênero/plataforma, paginação e estados vazios/erro.
- Reutilizar os tokens e classes já existentes (`superficie-papel`, `borda-embalagem`, `ficha-locacao`, `ficha-formulario`, `cabecalho-ficha`, `entrada-formulario`, `botao-formulario`, `aviso-formulario`, `etiqueta-promo`, `etiqueta-colada`, `selo-locadora`, `carimbo`, `prateleira`) e adicionar apenas as classes compartilhadas necessárias para cards de jogo, etiquetas de característica e opções de preferência.
- Preservar integralmente **textos visíveis**, validações, mensagens, estados de carregamento/erro/sucesso/vazio, atributos de acessibilidade (`aria-live`, `aria-pressed`, `aria-labelledby`, `aria-describedby`, `role`), rotas, guards, serviços, contratos da API e regras de negócio.
- Manter a **acessibilidade e a responsividade**: foco visível, navegação por teclado, contraste AA, layout sem rolagem horizontal em 390px e 1440px e respeito a `prefers-reduced-motion`.
- **Não** alterar `docs/prd.md` nem `docs/tdd.md`: o RF-19, o CA-23 e a DT-19 já registram a identidade visual entregue por etapas; esta change é a etapa seguinte. Recomendações, favoritos, página 404 e o restante do shell permanecem fora do escopo.

## Capabilities

### New Capabilities

<!-- Nenhuma capacidade nova: a change continua a capability de identidade visual já existente. -->

### Modified Capabilities

- `identidade-visual`: adiciona requisitos para a página de perfil gamer (`/perfil`) e para a página de catálogo (`/catalogo`), incluindo os estados de leitura/edição, busca/filtros, cards, paginação e estados vazios, preservando os fluxos dos RF-04 a RF-09 e os critérios CA-06 a CA-10 sob a identidade da locadora.

## Impact

- **Código afetado (web)**: `web/src/app/features/perfil/perfil.html`, `web/src/app/features/catalogo/catalogo.html` e `web/src/styles.css` (classes compartilhadas de card, etiqueta de característica e opção de preferência). Os arquivos `.ts` das duas features e os testes existentes não devem mudar, a menos que a reformulação revele uma dependência estrutural inevitável — nesse caso, o teste correspondente é ajustado sem enfraquecer o comportamento verificado.
- **Não afetado**: `api/` (contratos, endpoints, DTOs, modelos), serviços e guards de `web/src/app/core`, rotas (`app.routes.ts`), cabeçalho, rodapé e demais features.
- **Documentos**: PRD e TDD reutilizados sem alteração (RF-19/CA-23/DT-19 já cobrem a entrega por etapas); a justificativa será registrada no PR.
- **Dependências**: nenhuma nova (sem pacotes npm adicionais).
- **Rastreabilidade**: RF-19, CA-23 e DT-19 (identidade visual); RF-04 a RF-09, CA-06 a CA-10 (comportamento preservado); jornadas J-2 e J-3 do PRD.
- **Testes**: `npm test -- --watch=false` deve permanecer verde sem alteração de comportamento; a verificação visual e de interação será manual (perfil em leitura/edição, catálogo com resultados, busca sem resultados, catálogo vazio, erro e paginação, em desktop e móvel).

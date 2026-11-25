# Proposal

## Why

A identidade visual da locadora já cobre a fundação, o shell, a página inicial, a autenticação, a conta, o perfil e o catálogo. As páginas `/recomendacoes` e `/favoritos` continuam com a paleta genérica anterior (slate/emerald), justamente nas jornadas de descoberta e de coleção do jogador (J-5, J-6 e J-7 do PRD). Concluir o RF-19 nessas telas fecha a experiência de uso recorrente com a mesma linguagem visual, sem alterar os comportamentos já validados pelos RF-10 a RF-18 e seus critérios de aceite (CA-11 a CA-17).

## What Changes

- Reformular visualmente **`/recomendacoes`** com a linguagem da locadora: vitrine de sugestões, painel de Modo Descoberta, cards com capa, pontuação em selo, etiquetas de gênero, explicação por fatores e botão de favorito.
- Reformular visualmente **`/favoritos`** com a metáfora de ficha de locação: cabeçalho de coleção, cards de jogos marcados, avaliação em estrelas como carimbo, remoção e paginação em papel.
- Reutilizar os tokens e classes existentes (`superficie-papel`, `borda-embalagem`, `ficha-locacao`, `ficha-formulario`, `cabecalho-ficha`, `entrada-formulario`, `botao-formulario`, `aviso-formulario`, `etiqueta-promo`, `etiqueta-colada`, `selo-locadora`, `cartao-jogo`, `etiqueta-caracteristica`, `fita-adesiva`) e adicionar apenas as classes compartilhadas necessárias para fator de recomendação, selo de pontuação e botão de estrela.
- Preservar integralmente **textos visíveis**, ids (`#experiencia`), rótulos, mensagens, estados de carregamento/erro/perfil incompleto/vazio, atributos de acessibilidade (`aria-live`, `aria-pressed`, `aria-label`, `role`), rotas, guards, serviços, contratos da API e regras de negócio.
- Manter **acessibilidade e responsividade**: foco visível, navegação por teclado, contraste AA, layout sem rolagem horizontal em 390px e 1440px e respeito a `prefers-reduced-motion`.
- **Não** alterar `docs/prd.md` nem `docs/tdd.md`: o RF-19, o CA-23 e a DT-19 já registram a identidade visual entregue por etapas; esta change é a etapa seguinte. O algoritmo de recomendação, o backend e as demais páginas permanecem fora do escopo.

## Capabilities

### New Capabilities

<!-- Nenhuma capacidade nova: a change continua a capability de identidade visual já existente. -->

### Modified Capabilities

- `identidade-visual`: adiciona requisitos para a página de recomendações (`/recomendacoes`) e para a página de favoritos (`/favoritos`), incluindo Modo Descoberta, fatores explicados, estados de perfil incompleto/erro/vazio, avaliação em estrelas e paginação, preservando os fluxos dos RF-10 a RF-18 e os critérios CA-11 a CA-17 sob a identidade da locadora.

## Impact

- **Código afetado (web)**: `web/src/app/features/recomendacoes/recomendacoes.html`, `web/src/app/features/favoritos/favoritos.html` e `web/src/styles.css` (classes compartilhadas de fator de recomendação, selo de pontuação e botão de estrela). Os arquivos `.ts` das duas features e os testes existentes não devem mudar, a menos que a reformulação revele uma dependência estrutural inevitável — nesse caso, o teste correspondente é ajustado sem enfraquecer o comportamento verificado.
- **Não afetado**: `api/` (contratos, endpoints, DTOs, modelos e cálculo de recomendação), serviços e guards de `web/src/app/core`, rotas (`app.routes.ts`), cabeçalho, rodapé e demais features.
- **Documentos**: PRD e TDD reutilizados sem alteração (RF-19/CA-23/DT-19 já cobrem a entrega por etapas); a justificativa será registrada no PR.
- **Dependências**: nenhuma nova (sem pacotes npm adicionais).
- **Rastreabilidade**: RF-19, CA-23 e DT-19 (identidade visual); RF-10 a RF-18, CA-11 a CA-17 (comportamento preservado); jornadas J-5, J-6 e J-7 do PRD.
- **Testes**: `npm test -- --watch=false` deve permanecer verde sem alteração de comportamento; a verificação visual e de interação será manual (recomendações com resultados, Modo Descoberta, perfil incompleto, erro e vazio; favoritos com estrelas, remoção, vazio, erro e paginação, em desktop e móvel).

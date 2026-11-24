# Proposal

## Why

A change `identidade-visual-locadora` entregou a fundação visual, o shell e a página inicial, mas `/entrar`, `/criar-conta` e `/conta` continuam com a paleta genérica anterior (slate/emerald). Isso quebra a experiência justamente no fluxo de entrada e de gestão da conta (J-1 e J-8 do PRD), que é o primeiro contato do jogador com o produto. Continuar o RF-19 nessas telas fecha o circuito de autenticação com a identidade da locadora, sem alterar o comportamento já validado pelos RF-01 a RF-04 e seus critérios de aceite.

## What Changes

- Reformular visualmente **`/entrar`**, **`/criar-conta`** e **`/conta`** com a linguagem da locadora dos anos 90 (paleta, tipografia, materiais, etiquetas/selos e metáfora de balcão/ficha), aproveitando os tokens e classes já existentes (`superficie-papel`, `borda-embalagem`, `ficha-locacao`, `etiqueta-promo`, `selo-locadora`, `carimbo`).
- Adicionar **classes compartilhadas de formulário** no CSS global (campo, rótulo, entrada, botões primário/secundário e aviso), evitando repetir longas listas de utilitários nas três telas e mantendo foco visível, estados de erro/sucesso e contraste AA.
- Mover o acesso a **`/conta`** da navbar para dentro de **`/perfil`**: o item `Minha conta` deixa de existir no cabeçalho e passa a ser um botão visível na página de perfil, mantendo a rota e a proteção por sessão.
- Preservar integralmente os **textos visíveis**, validações, mensagens de erro/sucesso, estados de carregamento, links de alternância (`Criar conta`/`Entrar`), botão `Sair`, rotas, guards, serviços, contratos da API e regras de negócio.
- Manter a **acessibilidade e a responsividade**: rótulos associados aos campos, erros anunciáveis, foco visível por teclado, layout sem rolagem horizontal em 390px e 1440px e respeito a `prefers-reduced-motion`.
- **Não** alterar `docs/prd.md` nem `docs/tdd.md`: o RF-19, o CA-23 e a DT-19 já registram a identidade visual entregue por etapas; esta change é a etapa seguinte. Catálogo, recomendações, favoritos e página 404 permanecem fora do escopo.

## Capabilities

### New Capabilities

<!-- Nenhuma capacidade nova: a change continua a capability de identidade visual já existente. -->

### Modified Capabilities

- `identidade-visual`: adiciona requisitos para as páginas de autenticação (`/entrar` e `/criar-conta`), para a página de conta (`/conta`) e para o acesso à conta a partir do perfil; ajusta o requisito do shell para refletir a saída do atalho `Minha conta` da navbar, preservando os fluxos de cadastro, login e alteração de senha (RF-01 a RF-04; CA-01 a CA-04) sob a identidade da locadora.

## Impact

- **Código afetado (web)**: `web/src/app/features/entrar/entrar.html`, `web/src/app/features/criar-conta/criar-conta.html`, `web/src/app/features/conta/conta.html`, `web/src/app/layout/cabecalho/cabecalho.html`, `web/src/app/features/perfil/perfil.html` e `web/src/styles.css` (classes de formulário). Em TypeScript, muda apenas `perfil.ts` (import de `RouterLink`) e os testes `cabecalho.spec.ts`/`perfil.spec.ts`; os arquivos `.ts` de login, cadastro e conta não mudam.
- **Não afetado**: `api/` (contratos, endpoints, DTOs, modelos), serviços e guards de `web/src/app/core`, rotas (`app.routes.ts`), rodapé e demais features.
- **Documentos**: PRD e TDD reutilizados sem alteração (RF-19/CA-23/DT-19 já cobrem a entrega por etapas); a justificativa será registrada no PR.
- **Dependências**: nenhuma nova (sem pacotes npm adicionais).
- **Rastreabilidade**: RF-19, CA-23 e DT-19 (identidade visual); RF-01 a RF-04 e CA-01 a CA-04 (comportamento preservado); jornadas J-1 e J-8 do PRD.
- **Testes**: `npm test -- --watch=false` deve permanecer verde, com `cabecalho.spec.ts` ajustado à nova navegação e `perfil.spec.ts` cobrindo o atalho para `/conta`; a verificação visual e de interação será manual (visitante, autenticado, estados de erro/sucesso, desktop e móvel).

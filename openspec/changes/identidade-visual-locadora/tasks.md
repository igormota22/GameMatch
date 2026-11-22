# Tasks

## 1. Documentos globais e preparação

- [x] 1.1 Criar a branch `feature/identidade-visual-locadora` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna o nome da branch.
- [x] 1.2 Registrar o requisito de identidade visual (RF-19) e seu critério de aceite (CA-23) em `docs/prd.md`, rastreando aos RNF-03/05/07 existentes. **Verificação:** `Select-String -Path docs/prd.md -Pattern 'RF-19|CA-23'` encontra as entradas nas tabelas de §7 e §10.
- [x] 1.3 Registrar a DT-19 (design system da locadora sobre DT-04) em `docs/tdd.md`. **Verificação:** a linha DT-19 aparece na tabela §2 e referencia DT-04.

## 2. Fundação visual (design system)

- [ ] 2.1 Definir os tokens da identidade em `web/src/styles.css` com `@theme` do Tailwind v4 (cores da paleta da locadora, famílias tipográficas, raios e sombras) e verificar que `ng build` compila sem erro. **Verificação:** `npm run build` conclui com sucesso e as classes derivadas (ex.: cor nova) são usáveis nos templates.
- [ ] 2.2 Adicionar fontes livres auto-hospedadas (`woff2` subset PT-BR em `web/src/public/fontes/`) com `@font-face` e verificar que a página carrega as fontes sem depender de CDN. **Verificação:** `npm run build` inclui os arquivos de fonte e o `index.html`/CSS referencia-as localmente.
- [ ] 2.3 Criar as classes de materiais (texturas de papel/plástico, bordas e sombras de embalagem) e verificar o contraste WCAG AA dos pares texto/fundo principais da paleta. **Verificação:** revisão registrada dos pares de cor com razão ≥ 4.5:1 para texto normal.
- [ ] 2.4 Criar as classes de elementos gráficos de locadora (etiqueta, selo/adesivo, carimbo, ficha de locação) e verificar sua renderização sem regressão da suíte. **Verificação:** `npm test -- --watch=false` permanece com 81 testes verdes após a etapa.

## 3. Shell do site (cabeçalho e rodapé)

- [ ] 3.1 Reformular `web/src/app/app.html`, `layout/cabecalho/cabecalho.html` e `layout/rodape/rodape.html` com a identidade da locadora, preservando rótulos, rotas e estados de sessão. **Verificação:** `npm test -- --watch=false` mantém `cabecalho.spec.ts` verde (textos do menu e da marca inalterados).
- [ ] 3.2 Garantir o link de atribuição FreeToGame.com no rodapé em todas as rotas (CA-19). **Verificação:** revisão manual em `/`, `/entrar` e rota autenticada confirma link ativo; `npm test -- --watch=false` verde.
- [ ] 3.3 Adicionar interações de hover/foco no shell com `@media (hover: hover)` e `prefers-reduced-motion`. **Verificação:** navegação por teclado percorre o menu com foco visível e, com movimento reduzido no SO, não há animação decorativa.

## 4. Página inicial (fachada da locadora)

- [ ] 4.1 Reformular `web/src/app/features/inicio/inicio.html` como fachada/vitrine da locadora (chamada promocional, destaques e elementos gráficos) mantendo a marcação funcional. **Verificação:** `npm test -- --watch=false` mantém `inicio.spec.ts` verde.
- [ ] 4.2 Preservar os blocos funcionais na nova composição: resumo do perfil com CTAs, avisos de perfil incompleto e status da API. **Verificação:** `inicio.spec.ts` verde e verificação manual com usuário autenticado e visitante mostram todos os blocos operacionais.
- [ ] 4.3 Implementar as interações físicas da página inicial (hover de retirar caixa da prateleira, etiquetas ao passar o mouse, abertura de ficha) respeitando `prefers-reduced-motion`. **Verificação:** manual — efeitos aparecem no hover, conteúdo essencial visível sem hover e animações respeitam movimento reduzido.

## 5. Verificação integrada

- [ ] 5.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes (base: 81). **Verificação:** saída `81 passed`.
- [ ] 5.2 Executar `dotnet test api/GameMatch.sln` com o banco do compose ativo (`docker compose up -d db`) e confirmar suítes verdes (base: 107 unit + 67 integração). **Verificação:** saída `Aprovado!` nos dois projetos de teste.
- [ ] 5.3 Revisão visual manual (desktop e móvel) da página inicial e do shell nos estados visitante/autenticado: contraste, legibilidade, responsividade sem rolagem horizontal e navegação por teclado. **Verificação:** checklist registrado (capturas ou anotação) sem pendências.
- [ ] 5.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Revisar e aprovar as especificações antes de iniciar a implementação (nenhuma tarefa acima começa antes da aprovação).
- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

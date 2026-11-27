# Tasks

## 1. Preparação

- [x] 1.1 Criar a branch `feature/identidade-visual-pagina-nao-encontrada` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna `feature/identidade-visual-pagina-nao-encontrada`.

## 2. Página 404

- [x] 2.1 Reformular `web/src/app/features/nao-encontrado/nao-encontrado.html` com a ficha de atendimento da locadora, preservando o `h1` `Página não encontrada`, `Erro 404`, `O endereço acessado não existe ou foi movido.`, o link `Voltar para o início` para `/` e os elementos decorativos com `aria-hidden`. **Verificação:** inspeção do template confirma os textos e o `routerLink="/"`; `npm test -- --watch=false` permanece verde.
- [x] 2.2 Adicionar `web/src/app/features/nao-encontrado/nao-encontrado.spec.ts` verificando o `h1`, os textos `Erro 404` e `O endereço acessado não existe ou foi movido.` e o link `Voltar para o início` com destino `/`. **Verificação:** o novo teste passa em `npm test -- --watch=false`.
- [x] 2.3 Confirmar que `web/src/app/app.routes.ts` mantém a rota `**` pública, com título `GameMatch — Página não encontrada` e carregamento lazy, sem alterações no arquivo. **Verificação:** `git diff --exit-code web/src/app/app.routes.ts` sem saída.

## 3. Vitrine de favoritos e redirecionamento do cadastro

- [x] 3.1 Atualizar `web/src/app/features/inicio/inicio.ts` para injetar `FavoritosService` e, quando autenticado, carregar até 3 favoritos com `listar(1, 3)`, expondo os signals `favoritosVitrine`, `carregandoFavoritos` e `erroFavoritos`, sem alterar o carregamento de saúde e perfil. **Verificação:** inspeção do componente e `npm test -- --watch=false` verde.
- [x] 3.2 Reformular a vitrine em `web/src/app/features/inicio/inicio.html`: exibir até 3 favoritos com capa, `alt`, título e link para `/favoritos`; para visitante, `Entre para ver seus favoritos.` com link para `/entrar`; para autenticado sem favoritos, `Você ainda não tem jogos favoritos.` com link `Explorar catálogo` para `/catalogo`; em carregamento, `Carregando favoritos...` com `role="status"`; em erro, aviso com link para `/favoritos`. Remover o `aria-hidden` do contêiner que passa a ter conteúdo real. **Verificação:** inspeção do template e testes da home verdes.
- [x] 3.3 Atualizar `web/src/app/features/inicio/inicio.spec.ts` para responder a `/api/favoritos?pagina=1&tamanho=3` nos cenários autenticados e cobrir favoritos (até 3), visitante, vazio e erro, mantendo as asserções existentes de saúde e perfil. **Verificação:** `npm test -- --watch=false` verde.
- [x] 3.4 Alterar `web/src/app/features/criar-conta/criar-conta.ts` para redirecionar para `/perfil` após o cadastro, preservando a autenticação automática, as validações e as mensagens. **Verificação:** inspeção do componente e teste de redirecionamento verde.
- [x] 3.5 Adicionar `web/src/app/features/criar-conta/criar-conta.spec.ts` verificando que um cadastro válido autentica e chama `navigateByUrl('/perfil')`. **Verificação:** `npm test -- --watch=false` verde.

## 4. Verificação integrada

- [x] 4.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes, incluindo os testes da 404, da vitrine e do cadastro. **Verificação:** saída sem falhas.
- [x] 4.2 Executar `npm run build` em `web/` e confirmar a compilação de produção. **Verificação:** build concluído com sucesso.
- [x] 4.3 Revisão manual da 404, da vitrine da home e do redirecionamento do cadastro: identidade da locadora, shell e atribuição FreeToGame, foco visível por teclado, contraste AA, sem rolagem horizontal em 390px/1440px e animações desativadas com `prefers-reduced-motion`. **Verificação:** checklist registrado sem pendências.
- [x] 4.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, RF-01/CA-01, RF-16/CA-16, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

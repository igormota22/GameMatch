# Tasks

## 1. Preparação

- [x] 1.1 Criar a branch `feature/identidade-visual-pagina-nao-encontrada` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna `feature/identidade-visual-pagina-nao-encontrada`.

## 2. Página 404

- [x] 2.1 Reformular `web/src/app/features/nao-encontrado/nao-encontrado.html` com a ficha de atendimento da locadora, preservando o `h1` `Página não encontrada`, `Erro 404`, `O endereço acessado não existe ou foi movido.`, o link `Voltar para o início` para `/` e os elementos decorativos com `aria-hidden`. **Verificação:** inspeção do template confirma os textos e o `routerLink="/"`; `npm test -- --watch=false` permanece verde.
- [x] 2.2 Adicionar `web/src/app/features/nao-encontrado/nao-encontrado.spec.ts` verificando o `h1`, os textos `Erro 404` e `O endereço acessado não existe ou foi movido.` e o link `Voltar para o início` com destino `/`. **Verificação:** o novo teste passa em `npm test -- --watch=false`.
- [x] 2.3 Confirmar que `web/src/app/app.routes.ts` mantém a rota `**` pública, com título `GameMatch — Página não encontrada` e carregamento lazy, sem alterações no arquivo. **Verificação:** `git diff --exit-code web/src/app/app.routes.ts` sem saída.

## 3. Verificação integrada

- [x] 3.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes, incluindo o novo teste da página 404. **Verificação:** saída sem falhas.
- [x] 3.2 Executar `npm run build` em `web/` e confirmar a compilação de produção. **Verificação:** build concluído com sucesso.
- [x] 3.3 Revisão manual da página 404 em uma URL inexistente: identidade da locadora, shell e atribuição FreeToGame, foco visível por teclado, contraste AA, sem rolagem horizontal em 390px/1440px e animações desativadas com `prefers-reduced-motion`. **Verificação:** checklist registrado sem pendências.
- [ ] 3.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

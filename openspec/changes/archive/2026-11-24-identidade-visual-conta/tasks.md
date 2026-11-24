# Tasks

## 1. Preparação

- [x] 1.1 Criar a branch `feature/identidade-visual-conta` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna `feature/identidade-visual-conta`.

## 2. Classes compartilhadas de formulário

- [x] 2.1 Adicionar em `web/src/styles.css` (`@layer components`) as classes de formulário da locadora — `ficha-formulario`, `cabecalho-ficha`, `campo-formulario`, `rotulo-formulario`, `entrada-formulario`, `dica-formulario`, `erro-formulario`, `aviso-formulario` (erro/sucesso), `botao-formulario` (primário/secundário) e `link-formulario` — usando os tokens existentes. **Verificação:** `npm run build` conclui sem erro e as classes aparecem no CSS gerado.

## 3. Páginas de autenticação

- [x] 3.1 Reformular `web/src/app/features/entrar/entrar.html` com a ficha de balcão da locadora, preservando os textos `Entrar na sua conta`, `E-mail`, `Senha`, `Entrar`, `Ainda não tem conta?` e `Criar conta`, as validações, o estado `enviando`, a mensagem de erro e o `routerLink="/criar-conta"`. **Verificação:** `npm test -- --watch=false` verde e revisão manual do login (credenciais válidas, inválidas e validação local).
- [x] 3.2 Reformular `web/src/app/features/criar-conta/criar-conta.html` com a mesma linguagem, preservando os textos `Criar conta`, `E-mail`, `Senha`, a dica de senha, as mensagens de validação, o estado `enviando`, a mensagem de conflito e o `routerLink="/entrar"`. **Verificação:** `npm test -- --watch=false` verde e revisão manual do cadastro (sucesso, e-mail duplicado e validação local).

## 4. Página Minha conta

- [x] 4.1 Reformular `web/src/app/features/conta/conta.html` com fichas de balcão, preservando `Minha conta`, `Seus dados de acesso ao GameMatch.`, `Dados da conta`, `E-mail`, `Conta criada em`, `Alterar senha`, `Senha atual`, `Nova senha`, `Sair`, os estados de carregamento/erro/sucesso e o botão de saída. **Verificação:** `npm test -- --watch=false` verde e revisão manual de alteração de senha (sucesso e erro).

## 5. Acesso à conta pelo perfil

- [x] 5.1 Remover o item `Minha conta` do cabeçalho em `web/src/app/layout/cabecalho/cabecalho.html` e atualizar `cabecalho.spec.ts` para afirmar que o atalho não aparece no estado autenticado, mantendo os demais rótulos. **Verificação:** `npm test -- --watch=false` verde com `cabecalho.spec.ts` atualizado.
- [x] 5.2 Adicionar em `web/src/app/features/perfil/perfil.html` a ação `Minha conta` com `routerLink="/conta"` e incluir `RouterLink` nos imports de `web/src/app/features/perfil/perfil.ts`; ajustar `perfil.spec.ts` com `provideRouter([])` e uma verificação do link para `/conta`. **Verificação:** `npm test -- --watch=false` verde e revisão manual: `/perfil` leva a `/conta` e a rota continua protegida pelo guard.

## 6. Verificação integrada

- [x] 6.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes, incluindo os novos casos de cabeçalho e perfil. **Verificação:** saída sem falhas.
- [x] 6.2 Executar `npm run build` em `web/` e confirmar a compilação de produção. **Verificação:** build concluído com sucesso.
- [x] 6.3 Revisão manual de acessibilidade, responsividade e movimento reduzido nas três telas: foco visível por teclado, erros anunciáveis, sem rolagem horizontal em 390px/1440px e animações desativadas com `prefers-reduced-motion`. **Verificação:** checklist registrado sem pendências.
- [x] 6.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

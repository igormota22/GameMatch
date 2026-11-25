# Tasks

## 1. Preparação

- [ ] 1.1 Criar a branch `feature/identidade-visual-recomendacoes-favoritos` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna `feature/identidade-visual-recomendacoes-favoritos`.

## 2. Classes compartilhadas

- [ ] 2.1 Adicionar em `web/src/styles.css` (`@layer components`) as classes `selo-pontuacao`, `fator-recomendacao` e `botao-estrela`, usando apenas os tokens existentes, com foco visível, hover discreto, movimento seguro e estado `[aria-pressed="true"]` para as estrelas. **Verificação:** `npm run build` conclui sem erro e as classes aparecem no CSS gerado.

## 3. Recomendações

- [ ] 3.1 Reformular o cabeçalho e o painel do Modo Descoberta de `web/src/app/features/recomendacoes/recomendacoes.html`, preservando `Recomendações`, `Jogos feitos para o seu perfil`, `Cada sugestão explica por que combinou com as suas preferências.`, `Modo Descoberta`, a opção `Recomendações personalizadas`, o `select#experiencia` e o comportamento de recarga ao trocar a opção. **Verificação:** `npm test -- --watch=false` verde e revisão manual do seletor.
- [ ] 3.2 Reformular os cards de recomendação com capa, selo de pontuação, título, desenvolvedor, gêneros, bloco `Por que este jogo?`, descrição, fatores com `pontos`/`Sem correspondência` e botão de favorito, preservando a ordem dos cards, os `aria-label` de gêneros e de favorito, `aria-pressed`, `Favoritar`/`Favoritado` e as imagens com `alt` e `loading="lazy"`. **Verificação:** `npm test -- --watch=false` verde e revisão manual de favoritar/desfavoritar.
- [ ] 3.3 Reformular os estados de carregamento, perfil incompleto, erro e vazio, preservando `Carregando recomendações...`, `Perfil incompleto`, `Informe gêneros e plataformas no perfil para receber recomendações.`, o link `Configurar perfil` para `/perfil`, `Tentar novamente`, `Nenhuma recomendação disponível para o seu perfil no momento.` e os papéis `role="status"`/`role="alert"`, sem renderizar `<li>` no estado de perfil incompleto. **Verificação:** `npm test -- --watch=false` verde e revisão manual dos estados.

## 4. Favoritos

- [ ] 4.1 Reformular o cabeçalho, a contagem e os estados vazio e de erro de `web/src/app/features/favoritos/favoritos.html`, preservando `Favoritos`, `Seus jogos marcados`, `Favorite jogos para encontrá-los aqui e avalie de 1 a 5 estrelas o que já jogou.`, `Lista de favoritos`, a contagem em `aria-live="polite"`, `Você ainda não tem jogos favoritos.`, o link `Explorar catálogo` para `/catalogo`, `Tentar novamente` e os papéis `role="status"`/`role="alert"`. **Verificação:** `npm test -- --watch=false` verde e revisão manual de vazio/erro.
- [ ] 4.2 Reformular os cards de favoritos com capa, título, desenvolvedor, gêneros, bloco `Sua avaliação`, estrelas de 1 a 5, ação `Remover avaliação` e ação `Remover dos favoritos`, preservando os `aria-label` de avaliação e remoção, `aria-pressed` das estrelas, o preenchimento conforme a nota e o comportamento de avaliar/remover. **Verificação:** `npm test -- --watch=false` verde e revisão manual de avaliar/remover.
- [ ] 4.3 Reformular a paginação dos favoritos, preservando `Anterior`, `Página X de Y`, `Próxima`, o `aria-label="Paginação dos favoritos"` e os estados desabilitados. **Verificação:** `npm test -- --watch=false` verde e revisão manual da paginação.

## 5. Verificação integrada

- [ ] 5.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes sem alteração de comportamento. **Verificação:** saída sem falhas.
- [ ] 5.2 Executar `npm run build` em `web/` e confirmar a compilação de produção. **Verificação:** build concluído com sucesso.
- [ ] 5.3 Revisão manual de acessibilidade, responsividade e movimento reduzido em `/recomendacoes` e `/favoritos`: foco visível por teclado, estados anunciáveis, sem rolagem horizontal em 390px/1440px e animações desativadas com `prefers-reduced-motion`. **Verificação:** checklist registrado sem pendências.
- [ ] 5.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

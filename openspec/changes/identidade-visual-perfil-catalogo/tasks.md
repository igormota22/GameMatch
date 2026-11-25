# Tasks

## 1. Preparação

- [x] 1.1 Criar a branch `feature/identidade-visual-perfil-catalogo` a partir da `main` e verificar com `git branch --show-current` que é a branch ativa. **Verificação:** comando retorna `feature/identidade-visual-perfil-catalogo`.

## 2. Classes compartilhadas

- [ ] 2.1 Adicionar em `web/src/styles.css` (`@layer components`) as classes `cartao-jogo`, `etiqueta-caracteristica`, `etiqueta-caracteristica--plataforma` e `opcao-preferencia`, usando apenas os tokens existentes e com foco visível, hover discreto e movimento seguro. **Verificação:** `npm run build` conclui sem erro e as classes aparecem no CSS gerado.

## 3. Perfil gamer

- [x] 3.1 Reformular o modo de leitura de `web/src/app/features/perfil/perfil.html` como ficha do locatário, preservando `Perfil gamer`, `Jogador`, o nome de exibição, `Gêneros favoritos`, `Plataformas`, `Estilos de gameplay`, `Perfil completo`/`Perfil incompleto`, `Minha conta`, `Editar perfil`, os valores `Não informado`, as mensagens de estado vazio e a data de atualização. **Verificação:** `npm test -- --watch=false` verde e revisão manual do modo de leitura.
- [x] 3.2 Reformular o modo de edição com ficha, grupos de preferência, destaque de legados e ações, preservando os textos `Preferências`, `Campos com * são obrigatórios. As opções vêm do catálogo sincronizado.`, `Gêneros favoritos *`, `Plataformas *`, `Estilos de gameplay`, `O catálogo ainda não possui estilos de gameplay.`, `Opcional. Os estilos são as tags do catálogo.`, `Fora do catálogo (legado)`, `Cancelar`, `Salvar perfil`/`Salvando...`, as mensagens de validação/erro/sucesso, os `data-campo`/`data-valor`, os ids de erro e `aria-labelledby`/`aria-describedby`. **Verificação:** `npm test -- --watch=false` verde e revisão manual de edição, validação e falha de salvamento.

## 4. Catálogo

- [x] 4.1 Reformular o cabeçalho e a ficha de busca/filtros de `web/src/app/features/catalogo/catalogo.html`, preservando `Catálogo`, `Jogos gratuitos para descobrir`, `Buscar por título`, `Gênero`, `Plataforma`, `Tag`, os ids `#busca`, `#genero`, `#plataforma`, `#tag`, os valores `Todos`/`Todas` e o comportamento dos filtros. **Verificação:** `npm test -- --watch=false` verde e revisão manual de busca e filtros.
- [x] 4.2 Reformular a listagem em grade de cards de jogo com capa, título, gêneros, plataformas, desenvolvedor e botão de favorito, preservando `Jogos do catálogo`, a contagem em `aria-live="polite"`, `aria-label` dos grupos e do botão, `aria-pressed`, `Favoritar`/`Favoritado` e as imagens com `alt` e `loading="lazy"`. **Verificação:** `npm test -- --watch=false` verde e revisão manual de favoritar/desfavoritar.
- [x] 4.3 Reformular paginação, estados vazios, erro e atribuição, preservando `Anterior`, `Página X de Y`, `Próxima`, `Nenhum jogo encontrado para a busca e os filtros aplicados.`, `Limpar filtros`, `Não há jogos no catálogo. Peça ao operador para sincronizar o snapshot antes de navegar.`, `Tentar novamente`, `Dados fornecidos por` e o link `FreeToGame.com`. **Verificação:** `npm test -- --watch=false` verde e revisão manual dos estados vazio/sem resultado/erro.

## 5. Verificação integrada

- [x] 5.1 Executar `npm test -- --watch=false` em `web/` e confirmar todos os testes verdes sem alteração de comportamento. **Verificação:** saída sem falhas.
- [x] 5.2 Executar `npm run build` em `web/` e confirmar a compilação de produção. **Verificação:** build concluído com sucesso.
- [x] 5.3 Revisão manual de acessibilidade, responsividade e movimento reduzido em `/perfil` e `/catalogo`: foco visível por teclado, estados anunciáveis, sem rolagem horizontal em 390px/1440px e animações desativadas com `prefers-reduced-motion`. **Verificação:** checklist registrado sem pendências.
- [ ] 5.4 Abrir o Pull Request para a `main` com o template de `.github/pull_request_template.md`, relacionando RF-19/CA-23, DT-19, os requisitos da change e os testes executados. **Verificação:** PR aberto com o template preenchido.

## Workflow follow-up

- Arquivar a change após o merge do PR e a satisfação dos requisitos de revisão do projeto (`openspec archive`).

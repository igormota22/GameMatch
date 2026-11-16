# Tasks

## 1. Domínio e persistência do catálogo

- [ ] 1.1 Criar `Jogo` (`IdExterno` único, `Titulo`, `Descricao`, `Thumbnail`, `Desenvolvedor`, `UrlFonte`, `SincronizadoEm`), `JogoCaracteristica` e o enum `TipoCaracteristica` (`Genero = 1`, `Plataforma = 2`, `Estilo = 3`) em `GameMatch.Domain`; verificar com `dotnet build api/GameMatch.sln`.
- [ ] 1.2 Configurar EF para `jogos` e `jogos_caracteristicas` (chave composta `(jogo_id, tipo, valor)`, FK em cascata, índice único em `id_externo`, índice `(tipo, valor)`, enum como inteiro) e registrar os `DbSet`s no `GameMatchDbContext`; verificar que o model snapshot inclui as duas entidades.
- [ ] 1.3 Gerar a migration `CatalogoJogos` e conferir o script com `dotnet ef migrations script --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api`; verificar que o script cria as tabelas, os índices e a FK com `ON DELETE CASCADE`.
- [ ] 1.4 Aplicar a migration no PostgreSQL local com `dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api`; verificar no banco as tabelas, a chave composta e o índice único.
- [ ] 1.5 Atualizar a lista de tabelas criadas em `docs/ambiente.md` (`jogos` e `jogos_caracteristicas`) e conferir que o comando de migrations documentado continua executando como escrito.

## 2. CLI e coleta do snapshot

- [ ] 2.1 Criar o projeto console `api/src/GameMatch.Seed` (`net10.0`), adicionar `Microsoft.Extensions.Hosting` e `Microsoft.Extensions.Http` ao `Directory.Packages.props`, referenciar `GameMatch.Infrastructure` e incluir o projeto na solução com `dotnet sln api/GameMatch.sln add`; verificar com `dotnet build api/GameMatch.sln`.
- [ ] 2.2 Implementar a resolução de configuração (string de conexão por `--string-conexao`, `ConnectionStrings__GameMatch` ou `appsettings.json`) e o despacho dos subcomandos `coletar` e `sync`, com mensagem de uso e código de saída não zero para comando ausente ou conexão não configurada; verificar executando o CLI sem argumentos e com comando inválido.
- [ ] 2.3 Implementar o coletor: `GET /api/games`, iteração das tags candidatas de `Coleta:TagsCandidatas` via `/api/filter?tag=<tag>`, `User-Agent` identificando o projeto, timeout, tolerância a tag com erro ou vazia e escrita atômica dos três arquivos; verificar com testes unitários usando `HttpMessageHandler` fake para sucesso, tag 500 e falha de rede.
- [ ] 2.4 Executar `dotnet run --project api/src/GameMatch.Seed -- coletar` e versionar `seed/jogos.json`, `seed/tags.json` e `seed/metadados.json`; verificar que `metadados.json` registra a data, `totalJogos` igual ao tamanho do array de jogos, `totalTags` igual às chaves do mapa e as tags ignoradas.
- [ ] 2.5 Atualizar `seed/README.md` com o estado atual, os esquemas dos três arquivos e o procedimento de atualização (coletar → sincronizar); verificar que os comandos documentados executam como estão escritos.
- [ ] 2.6 Atualizar `docs/tdd.md` §5.4 e o entregável da F2 em §8 com o comando `coletar`, o esquema do snapshot e os fatos observados da fonte (416 jogos, gênero/plataforma únicos, tags candidatas e indisponíveis); verificar que o texto coincide com os arquivos gerados.

## 3. Sincronização do catálogo

- [ ] 3.1 Implementar a leitura e a validação do snapshot (arquivo ausente, JSON malformado, `id`/`title` ausentes, id externo duplicado, `coletadoEm` ausente) e o tratamento de tag órfã como aviso; verificar com testes unitários sobre fixtures em `api/tests/GameMatch.UnitTests/Catalogo/Fixtures`.
- [ ] 3.2 Implementar a sincronização transacional com upsert por `IdExterno`, substituição das características de cada jogo e resumo com inseridos, atualizados, tags ignoradas e data de coleta; verificar com teste de integração que executa o `sync` duas vezes sobre o mesmo diretório de fixtures e confirma que não há duplicatas e que a segunda execução reporta atualizações.
- [ ] 3.3 Garantir que snapshot inválido aborta antes da transação e preserva o banco; verificar com teste de integração que compara as contagens de `jogos` e `jogos_caracteristicas` antes e depois de uma sincronização com JSON inválido.
- [ ] 3.4 Documentar em `docs/ambiente.md` os comandos `coletar` e `sync`, a variável de conexão exigida e a limpeza manual de jogos removidos da fonte; verificar que os comandos documentados executam como escritos.

## 4. Aplicação e API do catálogo

- [ ] 4.1 Criar `JogoResumoDto`, `JogoDetalheDto`, `FiltrosCatalogoDto` e `PaginaDto<T>` e a validação de paginação (`pagina >= 1`, `tamanho` entre 1 e 100, padrão 24); verificar com testes unitários para valores válidos e inválidos.
- [ ] 4.2 Criar `ICatalogoRepositorio` e a implementação EF com consulta filtrada (busca `ILIKE` com escape de `\`, `%` e `_`; filtros exatos de gênero, plataforma e tag combinados com E lógico), ordenação por `Titulo` e `Id`, total sem paginação, listas distintas de filtros e leitura por id com características; verificar com `dotnet build api/GameMatch.sln`.
- [ ] 4.3 Criar `ServicoCatalogo` com listagem paginada, filtros e detalhe, retornando ausência para id inexistente; verificar com testes unitários usando repositório fake para paginação, combinação de filtros e detalhe.
- [ ] 4.4 Criar `CatalogoController` com `GET /catalogo`, `GET /catalogo/filtros` e `GET /catalogo/{id:int}`, `[Authorize]` e respostas 200/400/404 em ProblemDetails; registrar repositório e serviço na composição da API; verificar que as rotas aparecem no documento OpenAPI e que sem token a resposta é 401.
- [ ] 4.5 Criar testes de integração do contrato: 401 sem token, 400 para `pagina`/`tamanho` inválidos, primeira página com 24 itens e total, página além do total com lista vazia, catálogo vazio com total zero, detalhe 200 com características e `urlFonte`, 404 para id inexistente e 404 para identificador não numérico; verificar com `dotnet test api/GameMatch.sln --filter FullyQualifiedName~CatalogoTests`.
- [ ] 4.6 Criar testes de integração de busca e filtros: termo parcial sem diferenciar caixa, termo com curingas tratado literalmente, filtros isolados e combinados com busca, valor inexistente retornando lista vazia e opções de filtro distintas e ordenadas; verificar que cada cenário usa dados de fixture com termo/`IdExterno` único.
- [ ] 4.7 Atualizar a tabela de endpoints de `docs/ambiente.md` com `/catalogo`, `/catalogo/filtros` e `/catalogo/{id}`; verificar que métodos, códigos e parâmetros coincidem com o Swagger e com os testes de integração.

## 5. Frontend do catálogo

- [ ] 5.1 Criar `CatalogoService` em `core/servicos` com tipos do contrato, `listar(filtros)` e `obterFiltros()`; verificar com testes de `HttpTestingController` para URL, parâmetros de busca/paginação e método.
- [ ] 5.2 Criar `features/catalogo` com busca debounced, selects de gênero, plataforma e tag que voltam à página 1, grade de cards (imagem com `alt` e `loading="lazy"`, título, gêneros, plataformas, desenvolvedor), paginação com total e estados de carregamento e erro; verificar com testes do componente para carga, resultados, troca de filtro, paginação e erro da API.
- [ ] 5.3 Distinguir catálogo vazio (sem filtros) de busca sem resultados, com a ação “Limpar filtros” que restaura a listagem completa; verificar com testes do componente para cada estado e para o clique em limpar.
- [ ] 5.4 Garantir acessibilidade da tela (labels, navegação por teclado, `aria-live` com a contagem de resultados e foco na lista ao paginar) e conferir a atribuição FreeToGame no rodapé; verificar por inspeção do HTML renderizado nos testes e no navegador.
- [ ] 5.5 Adicionar a rota autenticada `/catalogo` em `app.routes.ts` e o link “Catálogo” no cabeçalho para usuários autenticados; verificar com os testes de guarda, rota e cabeçalho, incluindo o redirecionamento de visitante para `/entrar?destino=%2Fcatalogo`.
- [ ] 5.6 Atualizar a tabela de rotas do `README.md` e a árvore de projetos de `docs/arquitetura.md` com `features/catalogo` e `/catalogo`; verificar que a documentação coincide com `app.routes.ts`.
- [ ] 5.7 Executar `npm test -- --watch=false` e `npm run build` em `web/`; verificar que a suíte e o build terminam sem erros.

## 6. Verificação integrada da F2

- [ ] 6.1 Executar `dotnet test api/GameMatch.sln` e `npm test -- --watch=false`; verificar que todas as suítes passam.
- [ ] 6.2 Executar `sync` duas vezes seguidas sobre o snapshot versionado e consultar os três endpoints; verificar que o catálogo permanece sem duplicatas e que a paginação, a busca e os filtros respondem conforme o contrato.
- [ ] 6.3 Validar no navegador CA-08, CA-09 e CA-10: catálogo sincronizado com atribuição visível, busca e filtros combinados, catálogo vazio antes da sincronização e busca sem resultados com “Limpar filtros”; registrar evidência no PR.
- [ ] 6.4 Conferir que `seed/` permanece inalterado após usar a API e a interface e que a aplicação não escreve no diretório; verificar comparando `git status seed/` antes e depois do uso.

## Workflow follow-up

- Criar a feature branch `feature/catalogo` a partir da `main` e abrir o PR com o template de `.github/pull_request_template.md`, relacionando RF-07/RF-08/RF-09, DT-11/DT-15, CA-08/CA-09/CA-10 e TC-02.
- Arquivar a change somente após revisão, aprovações e merge do PR; sincronizar a spec `catalogo` na mesma etapa.

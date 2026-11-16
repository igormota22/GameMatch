# Tasks

## 1. Domínio do perfil

- [x] 1.1 Criar as entidades `PerfilGamer` e `PreferenciaPerfil` e o enum `TipoPreferencia` (`Genero = 1`, `Plataforma = 2`, `Estilo = 3`, `Experiencia = 4`) em `GameMatch.Domain`; verificar com `dotnet build api/GameMatch.sln`.
- [x] 1.2 Implementar a normalização de valores (trim, colapso de espaços e limite de 100 caracteres) e a deduplicação sem diferenciar maiúsculas, com testes unitários em `GameMatch.UnitTests`; verificar com `dotnet test api/GameMatch.sln --filter FullyQualifiedName~Preferencia`.
- [x] 1.3 Implementar a regra `EstaCompleto` (exige ao menos um gênero e uma plataforma; estilos e experiências não contam) com testes para perfil completo, incompleto e somente opcionais; verificar com `dotnet test api/GameMatch.sln --filter FullyQualifiedName~PerfilGamer`.

## 2. Persistência e migração

- [x] 2.1 Criar as configurações EF de `perfis_gamer` e `perfis_preferencias` (chave composta `(usuario_id, tipo, valor)`, FK para `AspNetUsers` com cascade, `valor` com 100 caracteres e enum como inteiro) e registrar os `DbSet`s no `GameMatchDbContext`; verificar com `dotnet build api/GameMatch.sln`.
- [x] 2.2 Gerar a migração `PerfilGamer` e conferir o script com `dotnet ef migrations script`; verificar que o script cria as duas tabelas, a chave primária composta e a FK com `ON DELETE CASCADE`.
- [x] 2.3 Aplicar a migração no PostgreSQL local com `dotnet ef database update --project api/src/GameMatch.Infrastructure --startup-project api/src/GameMatch.Api`; verificar as tabelas e restrições no banco com uma consulta ao catálogo do PostgreSQL.
- [x] 2.4 Atualizar a lista de tabelas criadas em `docs/ambiente.md` (`perfis_gamer` e `perfis_preferencias`) e conferir que o comando de migrations documentado continua executando como escrito.

## 3. Caso de uso e validação

- [x] 3.1 Criar `PerfilDto`, `PerfilSalvarDto` e o resultado de salvamento em `GameMatch.Application`, incluindo `completo` e `atualizadoEm` na leitura; verificar com `dotnet build api/GameMatch.sln`.
- [x] 3.2 Criar `IPerfilRepositorio` e `ServicoPerfil` com leitura sem perfil (listas vazias, `completo = false`, sem gravar) e salvamento por substituição; verificar com testes unitários usando repositório fake.
- [x] 3.3 Implementar a validação de gêneros e plataformas obrigatórios, rejeição de valores acima de 100 caracteres e deduplicação/normalização antes da gravação; verificar com testes unitários para cada caso de erro e para a preservação do perfil anterior.
- [x] 3.4 Cobrir a atualização de `AtualizadoEm`, a substituição completa das preferências e o cálculo de `completo` com testes unitários; verificar com `dotnet test api/GameMatch.sln --filter FullyQualifiedName~ServicoPerfil`.

## 4. API

- [x] 4.1 Criar `PerfilController` com `GET /perfil` e `PUT /perfil`, `[Authorize]`, identidade obtida do claim `sub` e respostas 200/401/422 em ProblemDetails; verificar que `/perfil` aparece no documento OpenAPI gerado em desenvolvimento.
- [x] 4.2 Registrar `IPerfilRepositorio` e `ServicoPerfil` na composição da API; verificar que a aplicação inicia e que `GET /perfil` sem token retorna 401.
- [x] 4.3 Criar testes de integração para 401, perfil vazio, criação, leitura, edição com substituição, 422 sem gênero/plataforma com perfil anterior preservado, normalização/deduplicação e isolamento entre tokens; verificar com `dotnet test api/GameMatch.sln --filter FullyQualifiedName~PerfilTests`.
- [x] 4.4 Atualizar a tabela de endpoints em `docs/ambiente.md` com `GET /perfil` e `PUT /perfil`; verificar que as rotas e os códigos documentados coincidem com o Swagger e com os testes de integração.

## 5. Frontend

- [x] 5.1 Criar `PerfilService` em `core/servicos` com `obter()` e `salvar()` e os tipos do contrato; verificar com testes de `HttpTestingController` para URL, método e corpo.
- [x] 5.2 Criar a feature `features/perfil` com formulário reativo (quatro áreas de texto, um valor por linha), validação de gêneros/plataformas, estados de carregamento/sucesso/erro e labels acessíveis; verificar com testes do componente para carga, salvamento, entrada inválida e erro da API.
- [x] 5.3 Adicionar a rota autenticada `/perfil` em `app.routes.ts` e o link “Perfil” no cabeçalho para usuários autenticados; verificar com o teste do cabeçalho e com o redirecionamento de visitante para `/entrar?destino=/perfil`.
- [x] 5.4 Atualizar `docs/arquitetura.md` e a tabela de rotas de `docs/ambiente.md` com `/perfil`; verificar que a documentação coincide com `app.routes.ts` e com o comportamento observado no navegador.
- [x] 5.5 Executar `npm test -- --watch=false` e `npm run build` em `web/`; verificar que a suíte e o build terminam sem erros.

## 6. Verificação integrada da F1

- [x] 6.1 Executar `dotnet test api/GameMatch.sln` e `npm test -- --watch=false`; verificar que todas as suítes passam.
- [x] 6.2 Validar manualmente CA-06: salvar perfil só com gêneros e plataformas, editar depois e confirmar que a leitura reflete a alteração; registrar evidência no PR.
- [x] 6.3 Validar manualmente CA-07 e CA-13 (preparação): confirmar que opcionais vazios são aceitos, que a interface mostra o perfil como incompleto sem preferências obrigatórias e completo depois de preenchê-las.
- [x] 6.4 Conferir no navegador que `/perfil` carrega com sessão, redireciona sem sessão e exibe mensagens em português em caso de erro da API; registrar evidência no PR.

## 7. Revisão da experiência de perfil (frontend)

- [x] 7.1 Refatorar `features/perfil` para abrir em modo de leitura, exibindo o nome de exibição derivado do e-mail da sessão (parte antes de `@`, com inicial maiúscula), as preferências atuais e o indicador de completude, com a ação “Editar perfil” que revela o formulário preenchido e retorna à leitura após salvar; atualizar os testes do componente para leitura, estado vazio, abertura da edição, salvamento e erro.
- [x] 7.2 Adicionar à tela de início uma seção autenticada com o resumo do perfil (nome de exibição derivado do e-mail, preferências, indicador de completude e atalho para `/perfil`), com estados de carregamento, vazio/incompleto e erro sem bloquear o restante da home; cobrir com testes do componente.
- [x] 7.3 Executar `npm test -- --watch=false` e `npm run build` em `web/`; validar no navegador o fluxo leitura → “Editar perfil” → salvamento e o resumo na home com e sem sessão; registrar evidência no PR.

## Workflow follow-up

- Criar a feature branch a partir da `main` e abrir o PR com o template de `.github/pull_request_template.md`, relacionando RF-05/RF-06, DT-06, CA-06/CA-07 e TC-01.
- Arquivar a change somente após revisão, aprovações e merge do PR.

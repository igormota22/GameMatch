# Tasks

## 1. API — contrato e vocabulário do catálogo (F2.5)

- [ ] 1.1 Remover `experiencias` de `PerfilDto`, `PerfilSalvarDto` e do mapeamento/salvamento de `ServicoPerfil`, ajustando os testes afetados; verificar com `dotnet build api/GameMatch.sln`.
- [ ] 1.2 Injetar `ICatalogoRepositorio` em `ServicoPerfil`, canonicalizar valores pela grafia do catálogo, rejeitar valores novos fora do catálogo com erros por campo e aceitar legado apenas quando já pertencer ao perfil atual; verificar com os testes unitários de `ServicoPerfilTests`.
- [ ] 1.3 Semear características do catálogo nos testes de integração de perfil e cobrir valor válido, valor fora do catálogo (422 sem gravar) e reenvio de legado; verificar com `dotnet test api/GameMatch.sln`.

## 2. Web — preferências a partir do catálogo

- [ ] 2.1 Remover `experiencias` do contrato em `perfil.service.ts` e da exibição em `features/inicio` e `features/perfil`, ajustando as specs; verificar com `npm test -- --watch=false`.
- [ ] 2.2 Substituir os campos de texto livre de `/perfil` por grupos de caixas de seleção carregados de `CatalogoService.obterFiltros()`, mantendo a obrigatoriedade de gênero e plataforma; verificar com a spec de `features/perfil` (opções renderizadas e contrato enviado).
- [ ] 2.3 Exibir valores salvos fora do catálogo em seção “Fora do catálogo (legado)”, permitindo desmarcá-los e enviando apenas os marcados; verificar com a spec de `features/perfil` (legado preservado e removível).
- [ ] 2.4 Tratar catálogo indisponível ou vazio na edição com aviso, nova tentativa e salvamento bloqueado, sem impedir a leitura do perfil; verificar com a spec de `features/perfil`.
- [ ] 2.5 Executar `npm test -- --watch=false` e confirmar todas as specs verdes.

## 3. Verificação integrada

- [ ] 3.1 Executar `dotnet test api/GameMatch.sln` e `npm test -- --watch=false` no estado final da branch e confirmar as suítes verdes (TC-01/CA-06/07/21/22).
- [ ] 3.2 Revisar manualmente `/perfil` com catálogo sincronizado: editar com opções, tentar valor fora do catálogo pela API (422) e remover um valor legado; registrar o resultado no PR.

## Workflow follow-up

- Abrir o Pull Request da branch `feature/preferencias-catalogo` para a `main` usando o template do repositório, relacionando RF-05/06, RB-16, CA-21/22 e DT-18.
- Arquivar a change somente após o merge e a revisão exigidos pelo projeto, sincronizando a spec `perfil-gamer`.

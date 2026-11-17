# catalogo Specification

## Purpose

Disponibilizar aos jogadores autenticados um catálogo de jogos gratuitos obtido
da FreeToGame por snapshot local, com pesquisa, filtros, paginação e estados
vazios claros, sustentando as recomendações das fases seguintes.

## Requirements

### Requirement: Coletar o snapshot do catálogo

Um operador DEVE (MUST) poder gerar o snapshot local do catálogo a partir da
fonte oficial FreeToGame, registrando data de coleta, contagens e origem, para
que a aplicação funcione offline. A API e a interface web NÃO DEVEM escrever em
`seed/`. Rastreia RF-07 e DT-11.

#### Scenario: Coleta bem-sucedida

- **WHEN** o operador executa a coleta com a fonte disponível
- **THEN** o snapshot é gravado com um registro por jogo retornado, o mapa de tags e os metadados com data, total de jogos, total de tags e origem

#### Scenario: Tag sem jogos ou indisponível

- **WHEN** uma tag candidata responde com erro ou não retorna jogos
- **THEN** a coleta prossegue, omite essa tag do mapa e registra a ocorrência nos metadados

#### Scenario: Falha de comunicação com a fonte

- **WHEN** a fonte oficial não responde durante a coleta
- **THEN** a coleta termina com mensagem clara e o snapshot anterior permanece intacto

#### Scenario: Aplicação não altera o snapshot

- **WHEN** a API ou a interface web executam qualquer operação de catálogo
- **THEN** nenhum arquivo de `seed/` é criado ou modificado

### Requirement: Sincronizar o catálogo a partir do snapshot

O catálogo do banco DEVE (MUST) ser populado exclusivamente pelo comando de
sincronização a partir do snapshot versionado, de forma idempotente por
identificador externo e transacional. Rastreia RF-07, RB-08, RB-12, DT-11 e
INV-03.

#### Scenario: Primeira sincronização

- **WHEN** o operador sincroniza um snapshot válido em um catálogo vazio
- **THEN** os jogos e suas características são inseridos e o resumo informa inseridos, atualizados e a data de coleta

#### Scenario: Sincronização repetida

- **WHEN** o operador sincroniza duas vezes o mesmo snapshot
- **THEN** nenhum jogo é duplicado e o segundo resumo informa os jogos como atualizados

#### Scenario: Características substituídas

- **WHEN** um snapshot deixa de atribuir uma tag a um jogo
- **THEN** após a sincronização o jogo deixa de apresentar essa tag e mantém as demais características

#### Scenario: Snapshot inválido

- **WHEN** um arquivo do snapshot está ausente, malformado ou com identificador duplicado
- **THEN** a sincronização aborta com mensagem que indica o arquivo e o motivo, sem alterar o banco

#### Scenario: Tag órfã

- **WHEN** o mapa de tags referencia um jogo que não existe no snapshot de jogos
- **THEN** a referência é ignorada e o resumo a reporta como aviso, sem interromper a sincronização

### Requirement: Controlar o acesso ao catálogo

Todos os endpoints do catálogo DEVEM (MUST) exigir token válido, respondendo
401 em ProblemDetails sem expor dados, e a rota web DEVE redirecionar visitantes
para o login preservando o destino. Rastreia RF-07, RB-04 e RNF-02.

#### Scenario: Requisição sem token

- **WHEN** uma requisição sem token válido consulta qualquer endpoint do catálogo
- **THEN** o sistema responde 401 com ProblemDetails e não expõe dados do catálogo

#### Scenario: Visitante na interface

- **WHEN** um visitante acessa `/catalogo`
- **THEN** é redirecionado para `/entrar` com destino `/catalogo`

#### Scenario: Sessão válida

- **WHEN** um usuário autenticado acessa `/catalogo`
- **THEN** a tela carrega o catálogo e mantém a navegação do aplicativo

### Requirement: Consultar o catálogo paginado

Usuário autenticado DEVE (MUST) consultar o catálogo em páginas de 24 itens por
padrão, com teto de 100, ordem estável e total de resultados, recebendo 400 para
parâmetros inválidos. Rastreia RF-07, DT-14 e CA-08.

#### Scenario: Consulta sem parâmetros

- **WHEN** o usuário consulta o catálogo sem informar paginação
- **THEN** recebe a primeira página com até 24 jogos em ordem estável, o tamanho e o total de jogos

#### Scenario: Página além do total

- **WHEN** o usuário solicita uma página posterior à última disponível
- **THEN** recebe 200 com lista vazia e o total de jogos preservado

#### Scenario: Parâmetros inválidos

- **WHEN** a página é menor que 1 ou o tamanho está fora de 1 a 100
- **THEN** o sistema responde 400 com ProblemDetails indicando o parâmetro inválido

#### Scenario: Catálogo não sincronizado

- **WHEN** o usuário consulta o catálogo antes de qualquer sincronização
- **THEN** recebe 200 com lista vazia e total zero

#### Scenario: Navegação entre páginas

- **WHEN** o usuário percorre as páginas do catálogo
- **THEN** nenhum jogo aparece em duas páginas e todos os jogos são alcançados

### Requirement: Pesquisar jogos por título

Usuário autenticado DEVE (MUST) pesquisar jogos pelo título, sem diferenciar
maiúsculas de minúsculas e tratando o termo como texto literal, sobre o catálogo
sincronizado. Rastreia RF-08 e RB-09.

#### Scenario: Termo parcial

- **WHEN** o usuário pesquisa por parte de um título existente
- **THEN** apenas os jogos cujo título contém o termo são retornados, independentemente da caixa

#### Scenario: Termo sem resultados

- **WHEN** o usuário pesquisa por um termo que não existe no catálogo
- **THEN** recebe 200 com lista vazia e total zero

#### Scenario: Caracteres curinga

- **WHEN** o usuário pesquisa por um termo contendo `%`, `_` ou `\`
- **THEN** o termo é interpretado literalmente e não como curinga de busca

### Requirement: Filtrar jogos por características

Usuário autenticado DEVE (MUST) filtrar o catálogo por gênero, plataforma e tag
e combinar esses filtros entre si e com a busca, retornando apenas jogos que
satisfaçam todas as condições. Rastreia RF-08, RB-09 e CA-09.

#### Scenario: Filtro isolado

- **WHEN** o usuário aplica um gênero, uma plataforma ou uma tag
- **THEN** recebe apenas os jogos que possuem aquela característica

#### Scenario: Filtros combinados

- **WHEN** o usuário aplica busca, gênero, plataforma e tag simultaneamente
- **THEN** recebe apenas os jogos que satisfazem todas as condições

#### Scenario: Filtro sem jogos

- **WHEN** o valor informado existe no catálogo, mas nenhum jogo satisfaz a combinação
- **THEN** recebe 200 com lista vazia e total zero

#### Scenario: Valor inexistente

- **WHEN** o usuário informa um valor de filtro que não existe no catálogo
- **THEN** recebe 200 com lista vazia, sem erro de validação

### Requirement: Listar as opções de filtro

Usuário autenticado DEVE (MUST) obter as listas de gêneros, plataformas e tags
presentes no catálogo sincronizado, sem repetições e em ordem alfabética.
Rastreia RF-08.

#### Scenario: Catálogo sincronizado

- **WHEN** o usuário consulta as opções de filtro com catálogo populado
- **THEN** recebe as listas de gêneros, plataformas e tags distintos existentes no catálogo

#### Scenario: Catálogo vazio

- **WHEN** o usuário consulta as opções de filtro sem catálogo sincronizado
- **THEN** recebe 200 com as três listas vazias

### Requirement: Consultar o detalhe de um jogo

Usuário autenticado DEVE (MUST) consultar os dados completos de um jogo do
catálogo, incluindo características e a URL da fonte FreeToGame, recebendo 404
quando o jogo não existir. Rastreia RF-07.

#### Scenario: Jogo existente

- **WHEN** o usuário consulta o identificador de um jogo do catálogo
- **THEN** recebe título, descrição, imagem, desenvolvedor, URL da fonte, gêneros, plataformas e tags

#### Scenario: Jogo inexistente

- **WHEN** o usuário consulta um identificador que não pertence ao catálogo
- **THEN** o sistema responde 404 com ProblemDetails

#### Scenario: Identificador inválido

- **WHEN** o usuário consulta `/catalogo/` com um identificador não numérico
- **THEN** o sistema responde 404 com ProblemDetails

### Requirement: Exibir o catálogo na interface

A rota autenticada `/catalogo` DEVE (MUST) apresentar os jogos com imagem,
título, gêneros, plataformas e desenvolvedor, permitir busca e filtros
combinados, paginar os resultados e distinguir catálogo vazio de busca sem
resultados, com atribuição à FreeToGame visível. Rastreia RF-07..09, CA-08..10 e
RNF-03.

#### Scenario: Catálogo sincronizado

- **WHEN** o usuário autenticado abre `/catalogo` com o catálogo populado
- **THEN** a tela exibe cards com imagem, título, gêneros, plataformas e desenvolvedor, além da atribuição à FreeToGame no rodapé

#### Scenario: Paginação na interface

- **WHEN** o catálogo tem mais de 24 jogos
- **THEN** a tela exibe uma página por vez, informa o total encontrado e permite ir à página anterior ou seguinte

#### Scenario: Catálogo não sincronizado

- **WHEN** o usuário abre `/catalogo` sem jogos sincronizados e sem filtros aplicados
- **THEN** a tela exibe um estado vazio informando que não há jogos no catálogo, sem oferecer limpar filtros

#### Scenario: Busca sem resultados

- **WHEN** a busca ou os filtros aplicados não retornam jogos
- **THEN** a tela exibe um estado vazio com a ação “Limpar filtros”, que restaura a listagem completa

#### Scenario: Falha da API

- **WHEN** a consulta do catálogo falha
- **THEN** a tela exibe mensagem de erro em português e mantém a navegação do aplicativo

#### Scenario: Acessibilidade da listagem

- **WHEN** o usuário navega pela tela apenas com teclado
- **THEN** busca, filtros e paginação têm rótulos acessíveis, são acionáveis por teclado e a contagem de resultados é anunciada

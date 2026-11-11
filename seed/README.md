# Seed do catálogo

Este diretório guardará o snapshot local do catálogo da FreeToGame, usado para
que a demonstração funcione sem depender de chamadas à API externa.

## Estado atual

Ainda não há snapshot. A sincronização será implementada na fase de catálogo.

## O que será versionado

- `jogos.json`: resposta consolidada de `https://www.freetogame.com/api/games`,
  com um registro por jogo retornado pela API.
- `tags.json`: mapa de tags reconstruído a partir das chamadas
  `https://www.freetogame.com/api/filter?tag=<tag>`, já que o payload de jogo não
  traz as tags.
- `metadados.json`: data da coleta, quantidade de jogos, quantidade de tags e
  origem dos dados.

## Regras

- Os arquivos são somente leitura para a aplicação: o banco é populado a partir
  deles, não o contrário.
- Toda alteração no snapshot deve registrar a data de coleta em
  `metadados.json`.
- Nenhum dado sensível ou credencial entra neste diretório.
- A origem dos dados é a FreeToGame (https://www.freetogame.com); a atribuição
  é exibida no rodapé da aplicação web.

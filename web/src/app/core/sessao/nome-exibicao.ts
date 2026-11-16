/**
 * Deriva o nome de exibição do usuário a partir do e-mail da sessão: usa a
 * parte antes de `@` com a inicial maiúscula, sem criar um campo de nome na
 * conta nesta fase.
 */
export function nomeDeExibicao(email: string | null | undefined): string {
  const parteLocal = email?.split('@')[0]?.trim();

  if (!parteLocal) {
    return '';
  }

  return parteLocal.charAt(0).toLocaleUpperCase('pt-BR') + parteLocal.slice(1);
}

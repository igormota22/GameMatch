import { nomeDeExibicao } from './nome-exibicao';

describe('nomeDeExibicao', () => {
  it('usa a parte antes do @ com a inicial maiúscula', () => {
    expect(nomeDeExibicao('igor@teste.com')).toBe('Igor');
  });

  it('preserva o restante da parte local', () => {
    expect(nomeDeExibicao('jogador.teste@teste.com')).toBe('Jogador.teste');
  });

  it('retorna texto vazio quando não há e-mail ou parte local', () => {
    expect(nomeDeExibicao(null)).toBe('');
    expect(nomeDeExibicao(undefined)).toBe('');
    expect(nomeDeExibicao('@teste.com')).toBe('');
  });
});

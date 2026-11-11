import { TestBed } from '@angular/core/testing';
import { Sessao, SessaoService } from './sessao.service';

const CHAVE_ARMAZENAMENTO = 'gamematch.sessao';

function criarSessao(expiraEm: Date): Sessao {
  return {
    token: 'token-teste',
    expiraEm: expiraEm.toISOString(),
    usuario: {
      email: 'jogador@teste.com',
      criadoEm: new Date('2026-01-01T00:00:00Z').toISOString(),
    },
  };
}

describe('SessaoService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.resetTestingModule();
  });

  it('inicia a sessão, expõe o usuário e persiste no localStorage', () => {
    const servico = TestBed.inject(SessaoService);

    servico.iniciar(criarSessao(new Date(Date.now() + 60_000)));

    expect(servico.autenticado()).toBe(true);
    expect(servico.usuario()?.email).toBe('jogador@teste.com');
    expect(servico.token()).toBe('token-teste');
    expect(localStorage.getItem(CHAVE_ARMAZENAMENTO)).toContain('token-teste');
  });

  it('encerra a sessão e remove o token do localStorage', () => {
    const servico = TestBed.inject(SessaoService);

    servico.iniciar(criarSessao(new Date(Date.now() + 60_000)));
    servico.encerrar();

    expect(servico.autenticado()).toBe(false);
    expect(servico.usuario()).toBeNull();
    expect(localStorage.getItem(CHAVE_ARMAZENAMENTO)).toBeNull();
  });

  it('restabelece a sessão válida salva ao carregar', () => {
    localStorage.setItem(
      CHAVE_ARMAZENAMENTO,
      JSON.stringify(criarSessao(new Date(Date.now() + 60_000))),
    );

    const servico = TestBed.inject(SessaoService);

    expect(servico.autenticado()).toBe(true);
  });

  it('descarta a sessão expirada e limpa o armazenamento', () => {
    localStorage.setItem(
      CHAVE_ARMAZENAMENTO,
      JSON.stringify(criarSessao(new Date(Date.now() - 60_000))),
    );

    const servico = TestBed.inject(SessaoService);

    expect(servico.autenticado()).toBe(false);
    expect(localStorage.getItem(CHAVE_ARMAZENAMENTO)).toBeNull();
  });
});

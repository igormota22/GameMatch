import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Cabecalho } from './cabecalho';
import { SessaoService } from '../../core/sessao/sessao.service';

describe('Cabecalho', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Cabecalho],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('exibe as opções de visitante', async () => {
    const fixture = TestBed.createComponent(Cabecalho);
    await fixture.whenStable();

    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(texto).toContain('GameMatch');
    expect(texto).toContain('Entrar');
    expect(texto).toContain('Criar conta');
    expect(texto).not.toContain('Catálogo');
    expect(texto).not.toContain('Recomendações');
    expect(texto).not.toContain('Favoritos');
    expect(texto).not.toContain('Sair');
  });

  it('exibe o estado autenticado com acesso ao catálogo, perfil, conta e saída', async () => {
    TestBed.inject(SessaoService).iniciar({
      token: 'token-teste',
      expiraEm: new Date(Date.now() + 60_000).toISOString(),
      usuario: { email: 'jogador@teste.com', criadoEm: new Date().toISOString() },
    });

    const fixture = TestBed.createComponent(Cabecalho);
    await fixture.whenStable();

    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(texto).toContain('Catálogo');
    expect(texto).toContain('Recomendações');
    expect(texto).toContain('Favoritos');
    expect(texto).toContain('Perfil');
    expect(texto).toContain('Minha conta');
    expect(texto).toContain('jogador@teste.com');
    expect(texto).toContain('Sair');
    expect(texto).not.toContain('Criar conta');
  });
});

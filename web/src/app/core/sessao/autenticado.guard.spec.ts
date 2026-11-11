import {
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { autenticadoGuard } from './autenticado.guard';
import { SessaoService } from './sessao.service';

describe('autenticadoGuard', () => {
  const rota = {} as ActivatedRouteSnapshot;
  const estado = { url: '/conta' } as RouterStateSnapshot;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  it('libera a rota quando existe sessão válida', () => {
    TestBed.inject(SessaoService).iniciar({
      token: 'token-teste',
      expiraEm: new Date(Date.now() + 60_000).toISOString(),
      usuario: { email: 'jogador@teste.com', criadoEm: new Date().toISOString() },
    });

    const resultado = TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado));

    expect(resultado).toBe(true);
  });

  it('redireciona o visitante para /entrar preservando o destino', () => {
    const resultado = TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado));

    expect(resultado instanceof UrlTree).toBe(true);
    expect((resultado as UrlTree).toString()).toBe('/entrar?destino=%2Fconta');
  });
});

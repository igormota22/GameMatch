import { routes } from './app.routes';
import { autenticadoGuard } from './core/sessao/autenticado.guard';

describe('rotas da aplicação', () => {
  it('expõe /catalogo como rota autenticada com carregamento lazy', () => {
    const rota = routes.find((candidata) => candidata.path === 'catalogo');

    expect(rota).toBeDefined();
    expect(rota?.canActivate).toContain(autenticadoGuard);
    expect(rota?.title).toBe('GameMatch — Catálogo');
    expect(typeof rota?.loadComponent).toBe('function');
  });

  it('mantém /perfil como rota autenticada', () => {
    const rota = routes.find((candidata) => candidata.path === 'perfil');

    expect(rota?.canActivate).toContain(autenticadoGuard);
  });
});

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SessaoService } from './sessao.service';

/**
 * Bloqueia rotas que exigem sessão, preservando o destino pretendido em
 * `?destino=` para retornar após o login.
 */
export const autenticadoGuard: CanActivateFn = (_rota, estado) => {
  const sessaoService = inject(SessaoService);
  const router = inject(Router);

  if (sessaoService.autenticado()) {
    return true;
  }

  return router.createUrlTree(['/entrar'], {
    queryParams: { destino: estado.url },
  });
};

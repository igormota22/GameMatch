import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { SessaoService } from './sessao.service';

/**
 * Anexa o token JWT às chamadas da API e encerra a sessão quando a API
 * responde 401 para uma requisição autenticada.
 */
export const autorizacaoInterceptor: HttpInterceptorFn = (requisicao, proximo) => {
  const sessaoService = inject(SessaoService);
  const token = sessaoService.token();

  const requisicaoAutenticada =
    token && requisicao.url.startsWith('/api')
      ? requisicao.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : requisicao;

  return proximo(requisicaoAutenticada).pipe(
    catchError((erro: unknown) => {
      if (erro instanceof HttpErrorResponse && erro.status === 401 && token) {
        sessaoService.encerrar();
      }

      return throwError(() => erro);
    }),
  );
};

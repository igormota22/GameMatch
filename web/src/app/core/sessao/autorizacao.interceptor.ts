
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { SessaoService } from './sessao.service';

const API_PRODUCAO = 'https://gamematch.runasp.net';

export const autorizacaoInterceptor: HttpInterceptorFn = (requisicao, proximo) => {
  const sessaoService = inject(SessaoService);
  const token = sessaoService.token();

  const ehRequisicaoApi = requisicao.url.startsWith('/api');

  const ehAmbienteLocal =
    typeof window !== 'undefined' &&
    ['localhost', '127.0.0.1'].includes(window.location.hostname);

  const urlApi =
    ehRequisicaoApi && !ehAmbienteLocal
      ? requisicao.url.replace(/^\/api/, API_PRODUCAO)
      : requisicao.url;

  const requisicaoConfigurada = requisicao.clone({
    url: urlApi,
    ...(ehRequisicaoApi && token
      ? { setHeaders: { Authorization: `Bearer ${token}` } }
      : {}),
  });

  return proximo(requisicaoConfigurada).pipe(
    catchError((erro: unknown) => {
      if (erro instanceof HttpErrorResponse && erro.status === 401 && token) {
        sessaoService.encerrar();
      }

      return throwError(() => erro);
    }),
  );
};
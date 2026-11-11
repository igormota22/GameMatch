import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetailsApi {
  detail?: string;
  title?: string;
}

/**
 * Extrai uma mensagem em português do ProblemDetails devolvido pela API,
 * cobrindo também falhas de rede.
 */
export function mensagemDeErro(erro: unknown): string {
  if (erro instanceof HttpErrorResponse) {
    const problema = erro.error as ProblemDetailsApi | null;

    if (problema?.detail) {
      return problema.detail;
    }

    if (erro.status === 0) {
      return 'Não foi possível contatar a API. Verifique se ela está em execução.';
    }

    if (erro.status === 401) {
      return 'E-mail ou senha incorretos.';
    }
  }

  return 'Não foi possível concluir a operação. Tente novamente.';
}

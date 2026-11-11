import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface VerificacaoSaude {
  status: string;
  descricao?: string | null;
  erro?: string | null;
  duracaoMs: number;
}

export interface RespostaSaude {
  status: string;
  duracaoTotalMs: number;
  verificacoes: Record<string, VerificacaoSaude>;
}

/**
 * Consulta o endpoint `/health` da API através do proxy de desenvolvimento.
 * O proxy encaminha `/api` para a API e remove o prefixo antes de repassar.
 */
@Injectable({ providedIn: 'root' })
export class SaudeService {
  private readonly http = inject(HttpClient);

  consultar(): Observable<RespostaSaude> {
    return this.http.get<RespostaSaude>('/api/health');
  }
}

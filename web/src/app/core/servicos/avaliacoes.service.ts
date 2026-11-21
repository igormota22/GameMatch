import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Avaliacao {
  nota: number;
}

/**
 * Consome os endpoints de avaliação do próprio usuário. A nota é um inteiro
 * de 1 a 5 e o salvamento cria ou substitui a avaliação existente (RB-15).
 */
@Injectable({ providedIn: 'root' })
export class AvaliacoesService {
  private readonly http = inject(HttpClient);

  obter(jogoId: number): Observable<Avaliacao> {
    return this.http.get<Avaliacao>(`/api/avaliacoes/${jogoId}`);
  }

  salvar(jogoId: number, nota: number): Observable<Avaliacao> {
    return this.http.put<Avaliacao>(`/api/avaliacoes/${jogoId}`, { nota });
  }

  remover(jogoId: number): Observable<void> {
    return this.http.delete<void>(`/api/avaliacoes/${jogoId}`);
  }
}

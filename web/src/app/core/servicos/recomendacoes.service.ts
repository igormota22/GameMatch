import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { JogoResumo } from './catalogo.service';

export interface FatorRecomendacao {
  chave: string;
  rotulo: string;
  contribuicao: number;
  correspondencias: string[];
}

export interface Recomendacao {
  jogo: JogoResumo;
  pontuacao: number;
  fatores: FatorRecomendacao[];
}

export interface RespostaRecomendacoes {
  itens: Recomendacao[];
}

export interface OpcaoDescoberta {
  chave: string;
  rotulo: string;
}

/**
 * Consome os endpoints de recomendação e do Modo Descoberta. A chave
 * `experiencia` só entra na query string quando informada (DT-13), de modo
 * que a API aplique o cálculo padrão caso contrário.
 */
@Injectable({ providedIn: 'root' })
export class RecomendacoesService {
  private readonly http = inject(HttpClient);

  listar(experiencia?: string): Observable<RespostaRecomendacoes> {
    let parametros = new HttpParams();

    const chave = experiencia?.trim();

    if (chave) {
      parametros = parametros.set('experiencia', chave);
    }

    return this.http.get<RespostaRecomendacoes>('/api/recomendacoes', {
      params: parametros,
    });
  }

  opcoes(): Observable<OpcaoDescoberta[]> {
    return this.http.get<OpcaoDescoberta[]>('/api/recomendacoes/opcoes');
  }
}

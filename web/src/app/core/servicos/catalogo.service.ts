import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface JogoResumo {
  id: number;
  titulo: string;
  thumbnail: string;
  generos: string[];
  plataformas: string[];
  desenvolvedor: string;
}

export interface PaginaJogos {
  items: JogoResumo[];
  pagina: number;
  tamanho: number;
  total: number;
}

export interface FiltrosCatalogo {
  generos: string[];
  plataformas: string[];
  tags: string[];
}

export interface ConsultaCatalogo {
  pagina?: number;
  tamanho?: number;
  busca?: string;
  genero?: string;
  plataforma?: string;
  tag?: string;
}

/**
 * Consome os endpoints do catálogo da API através do proxy `/api`. Os filtros
 * vazios são omitidos para que a API aplique os padrões de paginação.
 */
@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private readonly http = inject(HttpClient);

  listar(consulta: ConsultaCatalogo = {}): Observable<PaginaJogos> {
    let parametros = new HttpParams();

    for (const [chave, valor] of Object.entries(consulta)) {
      if (valor === null || valor === undefined || `${valor}`.trim().length === 0) {
        continue;
      }

      parametros = parametros.set(chave, `${valor}`);
    }

    return this.http.get<PaginaJogos>('/api/catalogo', { params: parametros });
  }

  obterFiltros(): Observable<FiltrosCatalogo> {
    return this.http.get<FiltrosCatalogo>('/api/catalogo/filtros');
  }
}

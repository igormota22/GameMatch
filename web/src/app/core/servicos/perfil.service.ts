import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Perfil {
  generos: string[];
  plataformas: string[];
  estilos: string[];
  completo: boolean;
  atualizadoEm: string | null;
}

export type PerfilSalvar = Pick<
  Perfil,
  'generos' | 'plataformas' | 'estilos'
>;

/**
 * Consome os endpoints de perfil da API através do proxy `/api`.
 */
@Injectable({ providedIn: 'root' })
export class PerfilService {
  private readonly http = inject(HttpClient);

  obter(): Observable<Perfil> {
    return this.http.get<Perfil>('/api/perfil');
  }

  salvar(perfil: PerfilSalvar): Observable<Perfil> {
    return this.http.put<Perfil>('/api/perfil', perfil);
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Credenciais {
  email: string;
  senha: string;
}

export interface UsuarioConta {
  email: string;
  criadoEm: string;
}

export interface RespostaAutenticacao {
  token: string;
  expiraEm: string;
  usuario: UsuarioConta;
}

/**
 * Consome os endpoints de conta da API através do proxy `/api`.
 */
@Injectable({ providedIn: 'root' })
export class ContaService {
  private readonly http = inject(HttpClient);

  cadastrar(credenciais: Credenciais): Observable<RespostaAutenticacao> {
    return this.http.post<RespostaAutenticacao>('/api/contas', credenciais);
  }

  entrar(credenciais: Credenciais): Observable<RespostaAutenticacao> {
    return this.http.post<RespostaAutenticacao>('/api/contas/login', credenciais);
  }

  obterConta(): Observable<UsuarioConta> {
    return this.http.get<UsuarioConta>('/api/conta');
  }

  alterarSenha(senhaAtual: string, novaSenha: string): Observable<void> {
    return this.http.put<void>('/api/conta/senha', { senhaAtual, novaSenha });
  }
}

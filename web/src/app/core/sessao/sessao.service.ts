import { Injectable, computed, signal } from '@angular/core';

export interface UsuarioSessao {
  email: string;
  criadoEm: string;
}

export interface Sessao {
  token: string;
  expiraEm: string;
  usuario: UsuarioSessao;
}

const CHAVE_ARMAZENAMENTO = 'gamematch.sessao';

/**
 * Mantém a sessão do usuário em um signal e persiste o token no localStorage.
 * Uma sessão expirada é descartada na leitura, mantendo o cabeçalho e as rotas
 * protegidas coerentes após recarregar a página.
 */
@Injectable({ providedIn: 'root' })
export class SessaoService {
  private readonly estado = signal<Sessao | null>(lerSessaoSalva());

  readonly sessao = this.estado.asReadonly();
  readonly autenticado = computed(() => this.estado() !== null);
  readonly usuario = computed(() => this.estado()?.usuario ?? null);
  readonly token = computed(() => this.estado()?.token ?? null);

  iniciar(sessao: Sessao): void {
    localStorage.setItem(CHAVE_ARMAZENAMENTO, JSON.stringify(sessao));
    this.estado.set(sessao);
  }

  encerrar(): void {
    localStorage.removeItem(CHAVE_ARMAZENAMENTO);
    this.estado.set(null);
  }
}

function lerSessaoSalva(): Sessao | null {
  const bruto = localStorage.getItem(CHAVE_ARMAZENAMENTO);

  if (!bruto) {
    return null;
  }

  try {
    const sessao = JSON.parse(bruto) as Sessao;

    if (!sessao?.token || !sessao.expiraEm || !sessao.usuario?.email) {
      localStorage.removeItem(CHAVE_ARMAZENAMENTO);
      return null;
    }

    if (new Date(sessao.expiraEm).getTime() <= Date.now()) {
      localStorage.removeItem(CHAVE_ARMAZENAMENTO);
      return null;
    }

    return sessao;
  } catch {
    localStorage.removeItem(CHAVE_ARMAZENAMENTO);
    return null;
  }
}

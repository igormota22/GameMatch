import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';
import { PaginaJogos } from './catalogo.service';

interface FavoritosIds {
  ids: number[];
}

/**
 * Mantém o conjunto de jogos favoritados do usuário em um signal, carregado
 * uma vez por sessão a partir de `GET /favoritos/ids` (D1). As operações de
 * favoritar/remover atualizam o estado de forma otimista e o revertem quando a
 * API falha, permitindo que os cards reflitam o estado sem recarregar a lista.
 */
@Injectable({ providedIn: 'root' })
export class FavoritosService {
  private readonly http = inject(HttpClient);
  private readonly estado = signal<ReadonlySet<number>>(new Set());
  private carregado = false;
  private requisicaoAtual: Observable<void> | null = null;

  readonly ids = this.estado.asReadonly();

  carregar(): Observable<void> {
    if (this.carregado) {
      return of(undefined);
    }

    this.requisicaoAtual ??= this.http.get<FavoritosIds>('/api/favoritos/ids').pipe(
      tap(({ ids }) => {
        this.estado.set(new Set(ids));
        this.carregado = true;
      }),
      map(() => undefined),
      catchError((erro: unknown) => {
        this.requisicaoAtual = null;

        return throwError(() => erro);
      }),
      finalize(() => {
        this.requisicaoAtual = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return this.requisicaoAtual;
  }

  estaFavoritado(jogoId: number): boolean {
    return this.estado().has(jogoId);
  }

  listar(pagina = 1, tamanho = 12): Observable<PaginaJogos> {
    return this.http.get<PaginaJogos>('/api/favoritos', {
      params: { pagina, tamanho },
    });
  }

  /**
   * Alterna o favorito de forma otimista. O observable conclui com o novo
   * estado; em falha, o estado anterior é restaurado antes do erro propagar.
   */
  alternar(jogoId: number): Observable<boolean> {
    const estavaFavoritado = this.estaFavoritado(jogoId);
    const novoEstado = !estavaFavoritado;

    this.aplicar(jogoId, novoEstado);

    const requisicao = estavaFavoritado
      ? this.http.delete<void>(`/api/favoritos/${jogoId}`)
      : this.http.post<void>(`/api/favoritos/${jogoId}`, null);

    return requisicao.pipe(
      map(() => novoEstado),
      catchError((erro: unknown) => {
        this.aplicar(jogoId, estavaFavoritado);

        return throwError(() => erro);
      }),
    );
  }

  /**
   * Remove o favorito de forma otimista, restaurando o estado em caso de falha.
   */
  remover(jogoId: number): Observable<void> {
    this.aplicar(jogoId, false);

    return this.http.delete<void>(`/api/favoritos/${jogoId}`).pipe(
      catchError((erro: unknown) => {
        this.aplicar(jogoId, true);

        return throwError(() => erro);
      }),
    );
  }

  /**
   * Descarta o estado em memória ao encerrar a sessão, evitando que outro
   * usuário veja os favoritos do anterior.
   */
  limpar(): void {
    this.estado.set(new Set());
    this.carregado = false;
    this.requisicaoAtual = null;
  }

  private aplicar(jogoId: number, favoritado: boolean): void {
    const proximo = new Set(this.estado());

    if (favoritado) {
      proximo.add(jogoId);
    } else {
      proximo.delete(jogoId);
    }

    this.estado.set(proximo);
  }
}

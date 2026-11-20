import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AvaliacoesService } from '../../core/servicos/avaliacoes.service';
import { JogoResumo, PaginaJogos } from '../../core/servicos/catalogo.service';
import { FavoritosService } from '../../core/servicos/favoritos.service';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';

/**
 * Tela de favoritos: lista paginada dos jogos marcados, remoção e controle de
 * estrelas 1–5 para avaliar, alterar e remover a avaliação. Reúne os estados
 * de carregando, vazio (CTA para o catálogo) e erro com nova tentativa —
 * RF-16..18 e CA-16/17.
 */
@Component({
  selector: 'app-favoritos',
  imports: [RouterLink],
  templateUrl: './favoritos.html',
})
export class Favoritos implements OnInit {
  private readonly favoritosService = inject(FavoritosService);
  private readonly avaliacoesService = inject(AvaliacoesService);

  protected readonly notasDisponiveis = [1, 2, 3, 4, 5];

  protected readonly pagina = signal<PaginaJogos | null>(null);
  protected readonly numeroPagina = signal(1);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly erroAcao = signal<string | null>(null);
  protected readonly avaliacoes = signal<ReadonlyMap<number, number>>(new Map());

  protected readonly total = computed(() => this.pagina()?.total ?? 0);
  protected readonly totalPaginas = computed(() => {
    const dados = this.pagina();

    if (!dados || dados.tamanho <= 0) {
      return 1;
    }

    return Math.max(1, Math.ceil(dados.total / dados.tamanho));
  });
  protected readonly temAnterior = computed(() => this.numeroPagina() > 1);
  protected readonly temProxima = computed(
    () => this.numeroPagina() < this.totalPaginas(),
  );
  protected readonly vazio = computed(
    () => !this.carregando() && !this.erro() && this.total() === 0,
  );
  protected readonly contagem = computed(() =>
    this.total() === 1 ? '1 jogo favoritado' : `${this.total()} jogos favoritados`,
  );

  ngOnInit(): void {
    this.favoritosService.carregar().subscribe({ error: () => undefined });
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.favoritosService.listar(this.numeroPagina()).subscribe({
      next: (pagina) => {
        this.pagina.set(pagina);
        this.carregarAvaliacoes(pagina.items);
        this.carregando.set(false);
      },
      error: (erro) => {
        this.pagina.set(null);
        this.erro.set(mensagemDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  protected anterior(): void {
    this.irParaPagina(this.numeroPagina() - 1);
  }

  protected proxima(): void {
    this.irParaPagina(this.numeroPagina() + 1);
  }

  protected notaAtual(jogoId: number): number | null {
    return this.avaliacoes().get(jogoId) ?? null;
  }

  protected avaliar(jogoId: number, nota: number): void {
    this.erroAcao.set(null);

    this.avaliacoesService.salvar(jogoId, nota).subscribe({
      next: ({ nota: salva }) => this.definirNota(jogoId, salva),
      error: (erro) => this.erroAcao.set(mensagemDeErro(erro)),
    });
  }

  protected removerAvaliacao(jogoId: number): void {
    this.erroAcao.set(null);

    this.avaliacoesService.remover(jogoId).subscribe({
      next: () => this.definirNota(jogoId, null),
      error: (erro) => this.erroAcao.set(mensagemDeErro(erro)),
    });
  }

  protected removerFavorito(jogoId: number): void {
    this.erroAcao.set(null);

    this.favoritosService.remover(jogoId).subscribe({
      next: () => {
        if (this.pagina()?.items.length === 1 && this.numeroPagina() > 1) {
          this.numeroPagina.set(this.numeroPagina() - 1);
        }

        this.carregar();
      },
      error: (erro) => this.erroAcao.set(mensagemDeErro(erro)),
    });
  }

  private irParaPagina(destino: number): void {
    if (destino < 1 || destino > this.totalPaginas()) {
      return;
    }

    this.numeroPagina.set(destino);
    this.carregar();
  }

  private carregarAvaliacoes(jogos: readonly JogoResumo[]): void {
    this.avaliacoes.set(new Map());

    for (const jogo of jogos) {
      this.avaliacoesService.obter(jogo.id).subscribe({
        next: ({ nota }) => this.definirNota(jogo.id, nota),
        // 404 significa que o jogo ainda não foi avaliado.
        error: () => undefined,
      });
    }
  }

  private definirNota(jogoId: number, nota: number | null): void {
    const proximo = new Map(this.avaliacoes());

    if (nota === null) {
      proximo.delete(jogoId);
    } else {
      proximo.set(jogoId, nota);
    }

    this.avaliacoes.set(proximo);
  }
}

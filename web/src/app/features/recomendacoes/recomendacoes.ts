import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import { FavoritosService } from '../../core/servicos/favoritos.service';
import {
  OpcaoDescoberta,
  Recomendacao,
  RecomendacoesService,
} from '../../core/servicos/recomendacoes.service';

/**
 * Tela de recomendações com Modo Descoberta: lista explicada por fatores,
 * seletor de experiência e os estados de carregando, perfil incompleto (CTA
 * para /perfil) e erro de API com navegação preservada (RNF-05).
 */
@Component({
  selector: 'app-recomendacoes',
  imports: [RouterLink],
  templateUrl: './recomendacoes.html',
})
export class Recomendacoes implements OnInit {
  private readonly recomendacoesService = inject(RecomendacoesService);

  protected readonly favoritos = inject(FavoritosService);

  protected readonly recomendacoes = signal<Recomendacao[]>([]);
  protected readonly opcoes = signal<OpcaoDescoberta[]>([]);
  protected readonly experiencia = signal('');
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly erroFavorito = signal<string | null>(null);
  protected readonly perfilIncompleto = signal(false);

  protected readonly semRecomendacoes = computed(
    () =>
      !this.carregando() &&
      !this.erro() &&
      !this.perfilIncompleto() &&
      this.recomendacoes().length === 0,
  );

  ngOnInit(): void {
    this.carregarOpcoes();
    this.carregar();
    this.carregarFavoritos();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.perfilIncompleto.set(false);

    this.recomendacoesService.listar(this.experiencia()).subscribe({
      next: (resposta) => {
        this.recomendacoes.set(resposta.itens);
        this.carregando.set(false);
      },
      error: (erro) => this.tratarErro(erro),
    });
  }

  protected selecionarExperiencia(event: Event): void {
    this.experiencia.set((event.target as HTMLSelectElement).value);
    this.carregar();
  }

  protected carregarFavoritos(): void {
    this.favoritos.carregar().subscribe({ error: () => undefined });
  }

  protected alternarFavorito(jogoId: number): void {
    this.erroFavorito.set(null);

    this.favoritos.alternar(jogoId).subscribe({
      error: (erro) => this.erroFavorito.set(mensagemDeErro(erro)),
    });
  }

  private carregarOpcoes(): void {
    this.recomendacoesService.opcoes().subscribe({
      next: (opcoes) => this.opcoes.set(opcoes),
      error: () => this.opcoes.set([]),
    });
  }

  private tratarErro(erro: unknown): void {
    this.recomendacoes.set([]);

    // 409 perfil-incompleto vira CTA de configuração em vez de mensagem
    // de falha genérica (CA-13).
    if (erro instanceof HttpErrorResponse && erro.status === 409) {
      this.perfilIncompleto.set(true);
    } else {
      this.erro.set(mensagemDeErro(erro));
    }

    this.carregando.set(false);
  }
}

import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { URL_FREETOGAME } from '../../core/configuracao/atribuicao';
import {
  CatalogoService,
  FiltrosCatalogo,
  PaginaJogos,
} from '../../core/servicos/catalogo.service';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';

const FILTROS_VAZIOS: FiltrosCatalogo = {
  generos: [],
  plataformas: [],
  tags: [],
};

@Component({
  selector: 'app-catalogo',
  imports: [ReactiveFormsModule],
  templateUrl: './catalogo.html',
})
export class Catalogo implements OnInit {
  private readonly formularioBuilder = inject(FormBuilder);
  private readonly catalogoService = inject(CatalogoService);
  private readonly lista = viewChild<ElementRef<HTMLElement>>('lista');

  protected readonly urlFreetogame = URL_FREETOGAME;
  protected readonly pagina = signal<PaginaJogos | null>(null);
  protected readonly filtros = signal<FiltrosCatalogo>(FILTROS_VAZIOS);
  protected readonly numeroPagina = signal(1);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly filtrosAtivos = signal(false);

  protected readonly total = computed(() => this.pagina()?.total ?? 0);
  protected readonly totalPaginas = computed(() => {
    const dados = this.pagina();

    if (!dados || dados.tamanho <= 0) {
      return 1;
    }

    return Math.max(1, Math.ceil(dados.total / dados.tamanho));
  });
  protected readonly temAnterior = computed(() => this.numeroPagina() > 1);
  protected readonly temProxima = computed(() => this.numeroPagina() < this.totalPaginas());
  protected readonly semResultados = computed(
    () => !this.carregando() && !this.erro() && this.total() === 0,
  );
  protected readonly buscaSemResultados = computed(
    () => this.semResultados() && this.filtrosAtivos(),
  );
  protected readonly contagem = computed(() =>
    this.total() === 1 ? '1 jogo encontrado' : `${this.total()} jogos encontrados`,
  );

  protected readonly formulario = this.formularioBuilder.nonNullable.group({
    busca: [''],
    genero: [''],
    plataforma: [''],
    tag: [''],
  });

  constructor() {
    this.formulario.controls.busca.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(() => this.filtrar());

    for (const controle of [
      this.formulario.controls.genero,
      this.formulario.controls.plataforma,
      this.formulario.controls.tag,
    ]) {
      controle.valueChanges
        .pipe(distinctUntilChanged(), takeUntilDestroyed())
        .subscribe(() => this.filtrar());
    }
  }

  ngOnInit(): void {
    this.carregar();
    this.carregarFiltros();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    const valores = this.formulario.getRawValue();
    const busca = valores.busca.trim();

    this.filtrosAtivos.set(
      busca.length > 0 ||
        valores.genero !== '' ||
        valores.plataforma !== '' ||
        valores.tag !== '',
    );

    this.catalogoService
      .listar({
        pagina: this.numeroPagina(),
        busca,
        genero: valores.genero,
        plataforma: valores.plataforma,
        tag: valores.tag,
      })
      .subscribe({
        next: (pagina) => {
          this.pagina.set(pagina);
          this.carregando.set(false);
        },
        error: (erro) => {
          this.pagina.set(null);
          this.erro.set(mensagemDeErro(erro));
          this.carregando.set(false);
        },
      });
  }

  protected carregarFiltros(): void {
    this.catalogoService.obterFiltros().subscribe({
      next: (filtros) => this.filtros.set(filtros),
      error: () => this.filtros.set(FILTROS_VAZIOS),
    });
  }

  protected limparFiltros(): void {
    this.formulario.reset(
      { busca: '', genero: '', plataforma: '', tag: '' },
      { emitEvent: false },
    );
    this.numeroPagina.set(1);
    this.carregar();
  }

  protected anterior(): void {
    this.irParaPagina(this.numeroPagina() - 1);
  }

  protected proxima(): void {
    this.irParaPagina(this.numeroPagina() + 1);
  }

  private filtrar(): void {
    this.numeroPagina.set(1);
    this.carregar();
  }

  private irParaPagina(destino: number): void {
    if (destino < 1 || destino > this.totalPaginas()) {
      return;
    }

    this.numeroPagina.set(destino);
    this.carregar();
    this.lista()?.nativeElement.focus();
  }
}

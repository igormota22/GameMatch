import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  CatalogoService,
  FiltrosCatalogo,
} from '../../core/servicos/catalogo.service';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import {
  Perfil as PerfilDados,
  PerfilService,
  PerfilSalvar,
} from '../../core/servicos/perfil.service';
import { nomeDeExibicao } from '../../core/sessao/nome-exibicao';
import { SessaoService } from '../../core/sessao/sessao.service';

type CampoPreferencia = 'generos' | 'plataformas' | 'estilos';

const FILTROS_VAZIOS: FiltrosCatalogo = {
  generos: [],
  plataformas: [],
  tags: [],
};

function aoMenosUmSelecionado(control: AbstractControl): ValidationErrors | null {
  const valores = control.value as string[] | null;

  return valores && valores.length > 0 ? null : { obrigatorio: true };
}

function chave(valor: string): string {
  return valor.toLocaleLowerCase('pt-BR');
}

function foraDoCatalogo(catalogo: string[], salvos: string[]): string[] {
  const disponiveis = new Set(catalogo.map(chave));

  return salvos.filter((valor) => !disponiveis.has(chave(valor)));
}

@Component({
  selector: 'app-perfil',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './perfil.html',
})
export class Perfil implements OnInit {
  private readonly formularioBuilder = inject(FormBuilder);
  private readonly perfilService = inject(PerfilService);
  private readonly catalogoService = inject(CatalogoService);
  private readonly sessaoService = inject(SessaoService);

  protected readonly perfil = signal<PerfilDados | null>(null);
  protected readonly modoEdicao = signal(false);
  protected readonly carregando = signal(true);
  protected readonly enviando = signal(false);
  protected readonly erroCarga = signal<string | null>(null);
  protected readonly mensagem = signal<string | null>(null);
  protected readonly erroSalvar = signal<string | null>(null);
  protected readonly filtros = signal<FiltrosCatalogo>(FILTROS_VAZIOS);
  protected readonly carregandoFiltros = signal(false);
  protected readonly erroFiltros = signal<string | null>(null);
  private readonly filtrosCarregados = signal(false);

  protected readonly completo = computed(() => this.perfil()?.completo ?? false);
  protected readonly atualizadoEm = computed(() => this.perfil()?.atualizadoEm ?? null);
  protected readonly nomeExibicao = computed(() =>
    nomeDeExibicao(this.sessaoService.usuario()?.email),
  );
  protected readonly semPreferencias = computed(() => {
    const perfil = this.perfil();

    return (
      perfil !== null &&
      perfil.generos.length === 0 &&
      perfil.plataformas.length === 0 &&
      perfil.estilos.length === 0
    );
  });
  protected readonly opcoes = computed(() => {
    const filtros = this.filtros();

    return {
      generos: filtros.generos,
      plataformas: filtros.plataformas,
      estilos: filtros.tags,
    };
  });
  protected readonly legados = computed(() => {
    const filtros = this.filtros();
    const perfil = this.perfil();

    return {
      generos: foraDoCatalogo(filtros.generos, perfil?.generos ?? []),
      plataformas: foraDoCatalogo(filtros.plataformas, perfil?.plataformas ?? []),
      estilos: foraDoCatalogo(filtros.tags, perfil?.estilos ?? []),
    };
  });
  protected readonly temLegados = computed(() =>
    Object.values(this.legados()).some((valores) => valores.length > 0),
  );
  protected readonly edicaoDisponivel = computed(() => {
    const filtros = this.filtros();

    return filtros.generos.length > 0 && filtros.plataformas.length > 0;
  });
  protected readonly salvamentoBloqueado = computed(
    () =>
      this.carregandoFiltros() ||
      this.erroFiltros() !== null ||
      !this.edicaoDisponivel(),
  );

  protected readonly formulario = this.formularioBuilder.nonNullable.group({
    generos: [[] as string[], [aoMenosUmSelecionado]],
    plataformas: [[] as string[], [aoMenosUmSelecionado]],
    estilos: [[] as string[]],
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erroCarga.set(null);

    this.perfilService.obter().subscribe({
      next: (perfil) => {
        this.perfil.set(perfil);
        this.preencher(perfil);
        this.modoEdicao.set(false);
        this.carregando.set(false);
      },
      error: (erro) => {
        this.erroCarga.set(mensagemDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  protected iniciarEdicao(): void {
    this.mensagem.set(null);
    this.erroSalvar.set(null);
    this.modoEdicao.set(true);

    if (!this.filtrosCarregados()) {
      this.carregarFiltros();
    }
  }

  protected carregarFiltros(): void {
    this.carregandoFiltros.set(true);
    this.erroFiltros.set(null);
    this.filtrosCarregados.set(false);

    this.catalogoService.obterFiltros().subscribe({
      next: (filtros) => {
        this.filtros.set(filtros);
        this.filtrosCarregados.set(true);
        this.carregandoFiltros.set(false);

        if (filtros.generos.length === 0 || filtros.plataformas.length === 0) {
          this.erroFiltros.set(
            'O catálogo ainda não possui opções suficientes para configurar o perfil. Peça ao operador para sincronizá-lo.',
          );
        }
      },
      error: (erro) => {
        this.erroFiltros.set(mensagemDeErro(erro));
        this.carregandoFiltros.set(false);
      },
    });
  }

  protected cancelarEdicao(): void {
    const perfil = this.perfil();

    if (perfil) {
      this.preencher(perfil);
    }

    this.erroSalvar.set(null);
    this.formulario.markAsPristine();
    this.formulario.markAsUntouched();
    this.modoEdicao.set(false);
  }

  protected alternar(
    campo: CampoPreferencia,
    valor: string,
    marcado: boolean,
  ): void {
    const controle = this.formulario.controls[campo];
    const atuais = controle.value;
    const alvo = chave(valor);
    const semValor = atuais.filter((item) => chave(item) !== alvo);

    controle.setValue(marcado ? [...semValor, valor] : semValor);
    controle.markAsDirty();
    controle.markAsTouched();
  }

  protected estaMarcado(campo: CampoPreferencia, valor: string): boolean {
    const alvo = chave(valor);

    return this.formulario.controls[campo].value.some(
      (item) => chave(item) === alvo,
    );
  }

  protected salvar(): void {
    if (this.salvamentoBloqueado()) {
      return;
    }

    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    if (this.enviando()) {
      return;
    }

    this.enviando.set(true);
    this.mensagem.set(null);
    this.erroSalvar.set(null);

    const valores = this.formulario.getRawValue();
    const entrada: PerfilSalvar = {
      generos: valores.generos,
      plataformas: valores.plataformas,
      estilos: valores.estilos,
    };

    this.perfilService.salvar(entrada).subscribe({
      next: (perfil) => {
        this.perfil.set(perfil);
        this.preencher(perfil);
        this.mensagem.set('Perfil salvo com sucesso.');
        this.modoEdicao.set(false);
        this.enviando.set(false);
      },
      error: (erro) => {
        this.erroSalvar.set(mensagemDeErro(erro));
        this.enviando.set(false);
      },
    });
  }

  protected listar(valores: string[]): string {
    return valores.length > 0 ? valores.join(', ') : 'Não informado';
  }

  protected formatarData(iso: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
      dateStyle: 'long',
      timeStyle: 'short',
    }).format(new Date(iso));
  }

  private preencher(perfil: PerfilDados): void {
    this.formulario.setValue({
      generos: [...perfil.generos],
      plataformas: [...perfil.plataformas],
      estilos: [...perfil.estilos],
    });
  }
}

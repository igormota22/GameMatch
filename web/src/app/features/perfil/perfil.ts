import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import {
  Perfil as PerfilDados,
  PerfilService,
  PerfilSalvar,
} from '../../core/servicos/perfil.service';
import { nomeDeExibicao } from '../../core/sessao/nome-exibicao';
import { SessaoService } from '../../core/sessao/sessao.service';

function dividirLinhas(valor: string): string[] {
  const vistos = new Set<string>();

  return valor
    .split('\n')
    .map((linha) => linha.trim())
    .filter((linha) => linha.length > 0)
    .filter((linha) => {
      const chave = linha.toLocaleLowerCase('pt-BR');

      if (vistos.has(chave)) {
        return false;
      }

      vistos.add(chave);
      return true;
    });
}

function aoMenosUmValor(control: AbstractControl): ValidationErrors | null {
  return dividirLinhas(String(control.value ?? '')).length > 0
    ? null
    : { obrigatorio: true };
}

@Component({
  selector: 'app-perfil',
  imports: [ReactiveFormsModule],
  templateUrl: './perfil.html',
})
export class Perfil implements OnInit {
  private readonly formularioBuilder = inject(FormBuilder);
  private readonly perfilService = inject(PerfilService);
  private readonly sessaoService = inject(SessaoService);

  protected readonly perfil = signal<PerfilDados | null>(null);
  protected readonly modoEdicao = signal(false);
  protected readonly carregando = signal(true);
  protected readonly enviando = signal(false);
  protected readonly erroCarga = signal<string | null>(null);
  protected readonly mensagem = signal<string | null>(null);
  protected readonly erroSalvar = signal<string | null>(null);

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
      perfil.estilos.length === 0 &&
      perfil.experiencias.length === 0
    );
  });

  protected readonly formulario = this.formularioBuilder.nonNullable.group({
    generos: ['', [Validators.required, aoMenosUmValor]],
    plataformas: ['', [Validators.required, aoMenosUmValor]],
    estilos: [''],
    experiencias: [''],
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

  protected salvar(): void {
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
      generos: dividirLinhas(valores.generos),
      plataformas: dividirLinhas(valores.plataformas),
      estilos: dividirLinhas(valores.estilos),
      experiencias: dividirLinhas(valores.experiencias),
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
      generos: perfil.generos.join('\n'),
      plataformas: perfil.plataformas.join('\n'),
      estilos: perfil.estilos.join('\n'),
      experiencias: perfil.experiencias.join('\n'),
    });
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ContaService, UsuarioConta } from '../../core/servicos/conta.service';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import { SessaoService } from '../../core/sessao/sessao.service';

@Component({
  selector: 'app-conta',
  imports: [ReactiveFormsModule],
  templateUrl: './conta.html',
})
export class Conta implements OnInit {
  private readonly formularioBuilder = inject(FormBuilder);
  private readonly contaService = inject(ContaService);
  private readonly sessaoService = inject(SessaoService);
  private readonly router = inject(Router);

  protected readonly conta = signal<UsuarioConta | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erroCarga = signal<string | null>(null);

  protected readonly formulario = this.formularioBuilder.nonNullable.group({
    senhaAtual: ['', [Validators.required]],
    novaSenha: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected readonly enviando = signal(false);
  protected readonly mensagem = signal<string | null>(null);
  protected readonly erroSenha = signal<string | null>(null);

  ngOnInit(): void {
    this.contaService.obterConta().subscribe({
      next: (conta) => {
        this.conta.set(conta);
        this.carregando.set(false);
      },
      error: (erro) => {
        this.erroCarga.set(mensagemDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  protected alterarSenha(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    if (this.enviando()) {
      return;
    }

    this.enviando.set(true);
    this.mensagem.set(null);
    this.erroSenha.set(null);

    const { senhaAtual, novaSenha } = this.formulario.getRawValue();

    this.contaService.alterarSenha(senhaAtual, novaSenha).subscribe({
      next: () => {
        this.formulario.reset();
        this.mensagem.set('Senha alterada com sucesso.');
        this.enviando.set(false);
      },
      error: (erro) => {
        this.erroSenha.set(mensagemDeErro(erro));
        this.enviando.set(false);
      },
    });
  }

  protected sair(): void {
    this.sessaoService.encerrar();
    this.router.navigateByUrl('/');
  }

  protected formatarData(iso: string): string {
    return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' }).format(new Date(iso));
  }
}

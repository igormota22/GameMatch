import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ContaService } from '../../core/servicos/conta.service';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import { SessaoService } from '../../core/sessao/sessao.service';

@Component({
  selector: 'app-criar-conta',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './criar-conta.html',
})
export class CriarConta {
  private readonly formularioBuilder = inject(FormBuilder);
  private readonly contaService = inject(ContaService);
  private readonly sessaoService = inject(SessaoService);
  private readonly router = inject(Router);

  protected readonly formulario = this.formularioBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    if (this.enviando()) {
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    const { email, senha } = this.formulario.getRawValue();

    this.contaService.cadastrar({ email, senha }).subscribe({
      next: (resposta) => {
        this.sessaoService.iniciar({
          token: resposta.token,
          expiraEm: resposta.expiraEm,
          usuario: resposta.usuario,
        });

        this.router.navigateByUrl('/conta');
      },
      error: (erro) => {
        this.erro.set(mensagemDeErro(erro));
        this.enviando.set(false);
      },
    });
  }
}

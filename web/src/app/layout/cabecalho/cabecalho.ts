import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SessaoService } from '../../core/sessao/sessao.service';

@Component({
  selector: 'app-cabecalho',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './cabecalho.html',
})
export class Cabecalho {
  private readonly sessaoService = inject(SessaoService);
  private readonly router = inject(Router);

  protected readonly autenticado = this.sessaoService.autenticado;
  protected readonly usuario = this.sessaoService.usuario;

  protected sair(): void {
    this.sessaoService.encerrar();
    this.router.navigateByUrl('/');
  }
}

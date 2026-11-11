import { Component, OnInit, inject, signal } from '@angular/core';
import { SaudeService } from '../../core/servicos/saude.service';

type EstadoSaude = 'carregando' | 'ok' | 'indisponivel';

@Component({
  selector: 'app-inicio',
  templateUrl: './inicio.html',
})
export class Inicio implements OnInit {
  private readonly saudeService = inject(SaudeService);

  protected readonly estado = signal<EstadoSaude>('carregando');
  protected readonly detalhe = signal('Consultando a API...');

  ngOnInit(): void {
    this.saudeService.consultar().subscribe({
      next: (resposta) => {
        const banco = resposta.verificacoes['postgresql']?.status;

        if (resposta.status === 'Healthy') {
          this.estado.set('ok');
          this.detalhe.set(`API e banco operacionais${banco ? ` (${banco})` : ''}.`);
          return;
        }

        this.estado.set('indisponivel');
        this.detalhe.set(`API respondeu com estado ${resposta.status}.`);
      },
      error: () => {
        this.estado.set('indisponivel');
        this.detalhe.set('Não foi possível contatar a API. Verifique se ela está em execução.');
      },
    });
  }
}

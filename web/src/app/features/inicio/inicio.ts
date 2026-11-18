import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { mensagemDeErro } from '../../core/servicos/mensagem-erro';
import { Perfil, PerfilService } from '../../core/servicos/perfil.service';
import { SaudeService } from '../../core/servicos/saude.service';
import { nomeDeExibicao } from '../../core/sessao/nome-exibicao';
import { SessaoService } from '../../core/sessao/sessao.service';

type EstadoSaude = 'carregando' | 'ok' | 'indisponivel';

@Component({
  selector: 'app-inicio',
  imports: [RouterLink],
  templateUrl: './inicio.html',
})
export class Inicio implements OnInit {
  private readonly saudeService = inject(SaudeService);
  private readonly perfilService = inject(PerfilService);
  private readonly sessaoService = inject(SessaoService);

  protected readonly estado = signal<EstadoSaude>('carregando');
  protected readonly detalhe = signal('Consultando a API...');

  protected readonly autenticado = this.sessaoService.autenticado;
  protected readonly nomeExibicao = computed(() =>
    nomeDeExibicao(this.sessaoService.usuario()?.email),
  );

  protected readonly perfil = signal<Perfil | null>(null);
  protected readonly carregandoPerfil = signal(false);
  protected readonly erroPerfil = signal<string | null>(null);
  protected readonly completo = computed(() => this.perfil()?.completo ?? false);
  protected readonly semPreferencias = computed(() => {
    const perfil = this.perfil();

    return (
      perfil !== null &&
      perfil.generos.length === 0 &&
      perfil.plataformas.length === 0 &&
      perfil.estilos.length === 0
    );
  });

  ngOnInit(): void {
    this.consultarSaude();

    if (this.autenticado()) {
      this.carregarPerfil();
    }
  }

  protected listar(valores: string[]): string {
    return valores.length > 0 ? valores.join(', ') : 'Não informado';
  }

  private consultarSaude(): void {
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

  private carregarPerfil(): void {
    this.carregandoPerfil.set(true);
    this.erroPerfil.set(null);

    this.perfilService.obter().subscribe({
      next: (perfil) => {
        this.perfil.set(perfil);
        this.carregandoPerfil.set(false);
      },
      error: (erro) => {
        this.erroPerfil.set(mensagemDeErro(erro));
        this.carregandoPerfil.set(false);
      },
    });
  }
}

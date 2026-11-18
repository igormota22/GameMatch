import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { Recomendacoes } from './recomendacoes';
import { autenticadoGuard } from '../../core/sessao/autenticado.guard';
import { SessaoService } from '../../core/sessao/sessao.service';
import { Recomendacao } from '../../core/servicos/recomendacoes.service';

const OPCOES = [
  { chave: 'relaxar', rotulo: 'Relaxar' },
  { chave: 'competir', rotulo: 'Competir' },
  { chave: 'explorar', rotulo: 'Explorar' },
  { chave: 'cooperar', rotulo: 'Cooperar' },
  { chave: 'historia', rotulo: 'História e imersão' },
];

function recomendacao(
  id: number,
  titulo: string,
  pontuacao: number,
  comExperiencia = false,
): Recomendacao {
  const fatores: Recomendacao['fatores'] = [
    { chave: 'generos', rotulo: 'Gêneros', contribuicao: 40, correspondencias: ['RPG'] },
    {
      chave: 'plataformas',
      rotulo: 'Plataformas',
      contribuicao: 20,
      correspondencias: ['PC (Windows)'],
    },
    { chave: 'estilos', rotulo: 'Estilos de gameplay', contribuicao: 0, correspondencias: [] },
  ];

  if (comExperiencia) {
    fatores.push({
      chave: 'experiencia',
      rotulo: 'Experiência',
      contribuicao: 15,
      correspondencias: ['RPG'],
    });
  }

  return {
    jogo: {
      id,
      titulo,
      thumbnail: `https://cdn.example/${id}.jpg`,
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      desenvolvedor: 'Estúdio Aurora',
    },
    pontuacao,
    fatores,
  };
}

describe('Recomendacoes', () => {
  let fixture: ComponentFixture<Recomendacoes>;
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [Recomendacoes],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Recomendacoes);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.removeItem('gamematch.sessao');
  });

  function elemento(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function botao(texto: string): HTMLButtonElement {
    const encontrado = Array.from(elemento().querySelectorAll('button')).find(
      (candidato) => candidato.textContent?.trim() === texto,
    );

    if (!encontrado) {
      throw new Error(`Botão "${texto}" não encontrado.`);
    }

    return encontrado;
  }

  function expectOpcoes(): void {
    http.expectOne('/api/recomendacoes/opcoes').flush(OPCOES);
  }

  async function carregarInicial(itens: Recomendacao[]): Promise<void> {
    fixture.detectChanges();

    expectOpcoes();

    http
      .expectOne(
        (requisicao) =>
          requisicao.url === '/api/recomendacoes' && !requisicao.params.has('experiencia'),
      )
      .flush({ itens });

    await fixture.whenStable();
    fixture.detectChanges();
  }

  it('exibe os cards ordenados com pontuação e fatores explicados', async () => {
    await carregarInicial([recomendacao(1, 'Aurora Tactics', 87), recomendacao(2, 'Blue Racer', 42)]);

    const texto = elemento().textContent ?? '';

    // Pontuação visível em cada card (CA-11).
    expect(texto).toContain('Aurora Tactics');
    expect(texto).toContain('87%');
    expect(texto).toContain('Blue Racer');
    expect(texto).toContain('42%');
    expect(texto.indexOf('Aurora Tactics')).toBeLessThan(texto.indexOf('Blue Racer'));

    // Fatores explicados com contribuição e correspondências (CA-12).
    expect(texto).toContain('Por que este jogo?');
    expect(texto).toContain('Gêneros');
    expect(texto).toContain('40 pontos');
    expect(texto).toContain('RPG');
    expect(texto).toContain('Plataformas');
    expect(texto).toContain('Sem correspondência');

    // O seletor vem de /recomendacoes/opcoes (CA-14).
    const select = elemento().querySelector('select#experiencia') as HTMLSelectElement;
    expect(select).not.toBeNull();

    const rotulos = Array.from(select.options).map((opcao) => opcao.textContent?.trim());
    expect(rotulos).toContain('Recomendações personalizadas');
    expect(rotulos).toContain('Explorar');
    expect(rotulos).toContain('História e imersão');
  });

  it('exibe CTA para /perfil quando a API responde perfil-incompleto', async () => {
    fixture.detectChanges();
    expectOpcoes();

    http
      .expectOne((requisicao) => requisicao.url === '/api/recomendacoes')
      .flush(
        { title: 'perfil-incompleto', detail: 'Perfil sem preferências mínimas.' },
        { status: 409, statusText: 'Conflict' },
      );

    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento().textContent ?? '';
    const cta = elemento().querySelector('a[href="/perfil"]');

    expect(texto).toContain('Perfil incompleto');
    expect(cta).not.toBeNull();
    expect(cta?.textContent?.trim()).toBe('Configurar perfil');
    expect(elemento().querySelectorAll('li')).toHaveLength(0);
  });

  it('exibe erro em português e permite tentar novamente', async () => {
    fixture.detectChanges();
    expectOpcoes();

    http
      .expectOne((requisicao) => requisicao.url === '/api/recomendacoes')
      .flush({ title: 'Erro interno' }, { status: 500, statusText: 'Server Error' });

    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento().textContent ?? '';
    expect(texto).toContain('Não foi possível concluir a operação. Tente novamente.');

    botao('Tentar novamente').click();
    fixture.detectChanges();

    http
      .expectOne((requisicao) => requisicao.url === '/api/recomendacoes')
      .flush({ itens: [recomendacao(1, 'Aurora Tactics', 87)] });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent ?? '').toContain('Aurora Tactics');
  });

  it('recarrega com a chave da experiência e mostra o fator de experiência', async () => {
    await carregarInicial([recomendacao(1, 'Aurora Tactics', 87)]);

    const select = elemento().querySelector('select#experiencia') as HTMLSelectElement;
    select.value = 'explorar';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    const requisicao = http.expectOne(
      (candidata) => candidata.url === '/api/recomendacoes',
    );
    expect(requisicao.request.params.get('experiencia')).toBe('explorar');
    requisicao.flush({ itens: [recomendacao(1, 'Aurora Tactics', 93, true)] });

    await fixture.whenStable();
    fixture.detectChanges();

    // A explicação passa a incluir o fator de experiência (CA-15).
    const texto = elemento().textContent ?? '';
    expect(texto).toContain('Experiência');
    expect(texto).toContain('15 pontos');
    expect(texto).toContain('93%');
  });

  it('exibe estado vazio quando a API não devolve recomendações', async () => {
    await carregarInicial([]);

    expect(elemento().textContent ?? '').toContain(
      'Nenhuma recomendação disponível para o seu perfil no momento.',
    );
  });
});

describe('acesso de visitante à rota de recomendações', () => {
  const rota = {} as ActivatedRouteSnapshot;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  it('redireciona o visitante de /recomendacoes para /entrar?destino=/recomendacoes', () => {
    const estado = { url: '/recomendacoes' } as RouterStateSnapshot;

    const resultado = TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado));

    expect(resultado instanceof UrlTree).toBe(true);
    expect((resultado as UrlTree).toString()).toBe('/entrar?destino=%2Frecomendacoes');
  });

  it('libera a rota quando existe sessão', () => {
    TestBed.inject(SessaoService).iniciar({
      token: 'token-teste',
      expiraEm: new Date(Date.now() + 60_000).toISOString(),
      usuario: { email: 'jogador@teste.com', criadoEm: new Date().toISOString() },
    });

    const estado = { url: '/recomendacoes' } as RouterStateSnapshot;

    const resultado = TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado));

    expect(resultado).toBe(true);
  });
});

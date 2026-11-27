import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { JogoResumo } from '../../core/servicos/catalogo.service';
import { Inicio } from './inicio';

const CHAVE_SESSAO = 'gamematch.sessao';
const FAVORITOS_URL = '/api/favoritos?pagina=1&tamanho=3';
const SAUDE_OK = {
  status: 'Healthy',
  duracaoTotalMs: 10,
  verificacoes: {
    postgresql: { status: 'Healthy', duracaoMs: 5 },
  },
};

interface PerfilResposta {
  generos: string[];
  plataformas: string[];
  estilos: string[];
  completo: boolean;
  atualizadoEm: string | null;
}

const PERFIL_COMPLETO: PerfilResposta = {
  generos: ['RPG', 'Ação'],
  plataformas: ['PC (Windows)'],
  estilos: ['turn-based'],
  completo: true,
  atualizadoEm: '2026-11-13T12:00:00Z',
};

function favorito(id: number, titulo: string): JogoResumo {
  return {
    id,
    titulo,
    thumbnail: `https://cdn.example/${id}.jpg`,
    generos: ['RPG'],
    plataformas: ['PC (Windows)'],
    desenvolvedor: `Estúdio ${id}`,
  };
}

describe('Inicio', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.removeItem(CHAVE_SESSAO);

    await TestBed.configureTestingModule({
      imports: [Inicio],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.removeItem(CHAVE_SESSAO);
  });

  function autenticar(email = 'igor@teste.com'): void {
    localStorage.setItem(
      CHAVE_SESSAO,
      JSON.stringify({
        token: 'token-de-teste',
        expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
        usuario: { email, criadoEm: new Date().toISOString() },
      }),
    );
  }

  function criar(): ComponentFixture<Inicio> {
    const fixture = TestBed.createComponent(Inicio);

    fixture.detectChanges();
    return fixture;
  }

  function elemento(fixture: ComponentFixture<Inicio>): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function responderSaude(): void {
    http.expectOne('/api/health').flush(SAUDE_OK);
  }

  function responderFavoritos(itens: JogoResumo[] = []): void {
    http.expectOne(FAVORITOS_URL).flush({
      items: itens,
      pagina: 1,
      tamanho: 3,
      total: itens.length,
    });
  }

  async function carregarPerfil(
    fixture: ComponentFixture<Inicio>,
    perfil: PerfilResposta,
    favoritos: JogoResumo[] = [],
  ): Promise<void> {
    http.expectOne('/api/perfil').flush(perfil);
    responderFavoritos(favoritos);
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function link(fixture: ComponentFixture<Inicio>, texto: string): HTMLAnchorElement | undefined {
    return Array.from(elemento(fixture).querySelectorAll('a')).find(
      (candidato) => candidato.textContent?.trim() === texto,
    );
  }

  it('não exibe o resumo do perfil para visitantes', async () => {
    const fixture = criar();

    responderSaude();
    await fixture.whenStable();
    fixture.detectChanges();

    http.expectNone('/api/perfil');
    http.expectNone(FAVORITOS_URL);

    const texto = elemento(fixture).textContent;

    expect(texto).not.toContain('Seu perfil');
    expect(texto).toContain('Status da API');
  });

  it('convida o visitante a entrar para ver os favoritos', async () => {
    const fixture = criar();

    responderSaude();
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Entre para ver seus favoritos.');
    expect(link(fixture, 'Entrar')?.getAttribute('href')).toBe('/entrar');
    expect(texto).not.toContain('Você ainda não tem jogos favoritos.');
  });

  it('exibe até três favoritos na vitrine', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, PERFIL_COMPLETO, [
      favorito(1, 'Aurora Tactics'),
      favorito(2, 'Blue Racer'),
      favorito(3, 'Cobalt Arena'),
    ]);

    const cartoes = elemento(fixture).querySelectorAll<HTMLAnchorElement>(
      'a[aria-label$="nos favoritos"]',
    );

    expect(cartoes.length).toBe(3);
    expect(cartoes[0].textContent).toContain('Aurora Tactics');
    expect(cartoes[0].getAttribute('aria-label')).toBe('Ver Aurora Tactics nos favoritos');
    expect(cartoes[0].querySelector('img')?.getAttribute('alt')).toBe('Capa de Aurora Tactics');
    expect(elemento(fixture).textContent).not.toContain('Você ainda não tem jogos favoritos.');
  });

  it('orienta a explorar o catálogo quando não há favoritos', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, PERFIL_COMPLETO);

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Você ainda não tem jogos favoritos.');
    expect(link(fixture, 'Explorar catálogo')?.getAttribute('href')).toBe('/catalogo');
  });

  it('mantém a home disponível quando os favoritos falham', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    http.expectOne('/api/perfil').flush(PERFIL_COMPLETO);
    http
      .expectOne(FAVORITOS_URL)
      .flush(
        { title: 'Erro', detail: 'Falha ao carregar os favoritos.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Falha ao carregar os favoritos.');
    expect(link(fixture, 'Ver favoritos')?.getAttribute('href')).toBe('/favoritos');
    expect(texto).toContain('Seu perfil');
    expect(texto).toContain('Status da API');
  });

  it('exibe o nome e as preferências do perfil completo', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, PERFIL_COMPLETO);

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Seu perfil');
    expect(texto).toContain('Igor');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('PC (Windows)');
    expect(texto).toContain('turn-based');
    expect(texto).toContain('Perfil completo');
    expect(link(fixture, 'Ver perfil completo')?.getAttribute('href')).toBe('/perfil');
    expect(link(fixture, 'Ver recomendações')?.getAttribute('href')).toBe('/recomendacoes');
    expect(link(fixture, 'Ver favoritos')?.getAttribute('href')).toBe('/favoritos');
  });

  it('orienta a configurar quando o perfil está incompleto', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, {
      generos: ['RPG'],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Perfil incompleto');
    expect(texto).toContain('Informe ao menos um gênero e uma plataforma');
    expect(texto).toContain('Não informado');
    expect(link(fixture, 'Configurar perfil')?.getAttribute('href')).toBe('/perfil');
    expect(link(fixture, 'Ver recomendações')?.getAttribute('href')).toBe('/recomendacoes');
    expect(link(fixture, 'Ver favoritos')?.getAttribute('href')).toBe('/favoritos');
  });

  it('orienta a configurar quando o perfil está vazio', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, {
      generos: [],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Você ainda não informou preferências.');
    expect(link(fixture, 'Configurar perfil')?.getAttribute('href')).toBe('/perfil');
  });

  it('mantém a home disponível e oferece atalho quando a leitura do perfil falha', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    http
      .expectOne('/api/perfil')
      .flush(
        { title: 'Erro', detail: 'Falha ao carregar o perfil.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );
    responderFavoritos();
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Falha ao carregar o perfil.');
    expect(link(fixture, 'Abrir perfil')?.getAttribute('href')).toBe('/perfil');
    expect(texto).toContain('Status da API');
    expect(texto).toContain('API e banco operacionais');
  });
});

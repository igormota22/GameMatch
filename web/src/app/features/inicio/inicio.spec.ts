import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Inicio } from './inicio';

const CHAVE_SESSAO = 'gamematch.sessao';
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

  async function carregarPerfil(
    fixture: ComponentFixture<Inicio>,
    perfil: PerfilResposta,
  ): Promise<void> {
    http.expectOne('/api/perfil').flush(perfil);
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

    const texto = elemento(fixture).textContent;

    expect(texto).not.toContain('Seu perfil');
    expect(texto).toContain('Status da API');
  });

  it('exibe o nome e as preferências do perfil completo', async () => {
    autenticar();

    const fixture = criar();

    responderSaude();
    await carregarPerfil(fixture, {
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Seu perfil');
    expect(texto).toContain('Igor');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('PC (Windows)');
    expect(texto).toContain('turn-based');
    expect(texto).toContain('Perfil completo');
    expect(link(fixture, 'Ver perfil completo')?.getAttribute('href')).toBe('/perfil');
    expect(link(fixture, 'Ver recomendações')?.getAttribute('href')).toBe('/recomendacoes');
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
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento(fixture).textContent;

    expect(texto).toContain('Falha ao carregar o perfil.');
    expect(link(fixture, 'Abrir perfil')?.getAttribute('href')).toBe('/perfil');
    expect(texto).toContain('Status da API');
    expect(texto).toContain('API e banco operacionais');
  });
});

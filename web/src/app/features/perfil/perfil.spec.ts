import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FiltrosCatalogo } from '../../core/servicos/catalogo.service';
import { Perfil } from './perfil';

const CHAVE_SESSAO = 'gamematch.sessao';

const FILTROS: FiltrosCatalogo = {
  generos: ['RPG', 'Ação', 'Estratégia'],
  plataformas: ['PC (Windows)', 'Web Browser'],
  tags: ['turn-based', 'co-op'],
};

interface PerfilResposta {
  generos: string[];
  plataformas: string[];
  estilos: string[];
  completo: boolean;
  atualizadoEm: string | null;
}

describe('Perfil', () => {
  let fixture: ComponentFixture<Perfil>;
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.setItem(
      CHAVE_SESSAO,
      JSON.stringify({
        token: 'token-de-teste',
        expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
        usuario: { email: 'igor@teste.com', criadoEm: new Date().toISOString() },
      }),
    );

    await TestBed.configureTestingModule({
      imports: [Perfil],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Perfil);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.removeItem(CHAVE_SESSAO);
  });

  function elemento(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function caixa(campo: string, valor: string): HTMLInputElement {
    const encontrada = elemento().querySelector<HTMLInputElement>(
      `input[type="checkbox"][data-campo="${campo}"][data-valor="${valor}"]`,
    );

    if (!encontrada) {
      throw new Error(`Opção "${valor}" do campo "${campo}" não encontrada.`);
    }

    return encontrada;
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

  async function carregarCom(perfil: PerfilResposta): Promise<void> {
    fixture.detectChanges();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush(perfil);
    await fixture.whenStable();
    fixture.detectChanges();
  }

  async function abrirEdicao(filtros: FiltrosCatalogo = FILTROS): Promise<void> {
    botao('Editar perfil').click();
    fixture.detectChanges();

    const requisicao = http.expectOne('/api/catalogo/filtros');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush(filtros);
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function enviar(): void {
    elemento().querySelector('form')?.dispatchEvent(new Event('submit'));
  }

  async function aguardarEstabilidade(): Promise<void> {
    await fixture.whenStable();
    fixture.detectChanges();
  }

  it('exibe o nome de exibição e as preferências atuais em modo de leitura', async () => {
    await carregarCom({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });

    const texto = elemento().textContent;

    expect(texto).toContain('Jogador');
    expect(texto).toContain('Igor');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('PC (Windows)');
    expect(texto).toContain('turn-based');
    expect(texto).toContain('Perfil completo');
    expect(texto).not.toContain('Perfil incompleto');
    expect(botao('Editar perfil')).toBeTruthy();
    expect(elemento().querySelector('input[type="checkbox"]')).toBeNull();
  });

  it('exibe orientação e valores não informados quando o perfil está vazio', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    const texto = elemento().textContent;

    expect(texto).toContain('Perfil incompleto');
    expect(texto).toContain('Você ainda não informou preferências.');
    expect(texto).toContain('Perfil ainda não salvo');
    expect(texto?.match(/Não informado/g)).toHaveLength(3);
    expect(botao('Editar perfil')).toBeTruthy();
  });

  it('abre a edição com as opções do catálogo já marcadas a partir do perfil', async () => {
    await carregarCom({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });

    await abrirEdicao();

    expect(caixa('generos', 'RPG').checked).toBe(true);
    expect(caixa('generos', 'Ação').checked).toBe(false);
    expect(caixa('plataformas', 'PC (Windows)').checked).toBe(true);
    expect(caixa('estilos', 'turn-based').checked).toBe(true);
    expect(elemento().querySelector('textarea')).toBeNull();
    expect(botao('Salvar perfil').disabled).toBe(false);
  });

  it('salva as opções marcadas sem enviar o campo de experiências', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    await abrirEdicao();

    caixa('generos', 'RPG').click();
    caixa('generos', 'Ação').click();
    caixa('plataformas', 'PC (Windows)').click();
    caixa('estilos', 'turn-based').click();
    fixture.detectChanges();

    enviar();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
    });

    requisicao.flush({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });
    await aguardarEstabilidade();

    const texto = elemento().textContent;

    expect(texto).toContain('Perfil salvo com sucesso.');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('Perfil completo');
    expect(elemento().querySelector('input[type="checkbox"]')).toBeNull();
    expect(botao('Editar perfil')).toBeTruthy();
  });

  it('bloqueia o envio e mostra as mensagens quando os obrigatórios estão vazios', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    await abrirEdicao();

    enviar();
    await aguardarEstabilidade();

    http.expectNone('/api/perfil');
    expect(elemento().textContent).toContain('Informe ao menos um gênero.');
    expect(elemento().textContent).toContain('Informe ao menos uma plataforma.');
    expect(caixa('generos', 'RPG').checked).toBe(false);
  });

  it('mantém os valores marcados quando o salvamento falha', async () => {
    await carregarCom({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      completo: true,
      atualizadoEm: null,
    });

    await abrirEdicao();

    caixa('generos', 'Ação').click();
    caixa('generos', 'RPG').click();

    enviar();

    const requisicao = http.expectOne('/api/perfil');

    requisicao.flush(
      {
        title: 'Perfil inválido',
        detail: 'Revise as preferências informadas.',
        status: 422,
      },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await aguardarEstabilidade();

    expect(elemento().textContent).toContain('Revise as preferências informadas.');
    expect(elemento().textContent).not.toContain('Perfil salvo com sucesso.');
    expect(caixa('generos', 'Ação').checked).toBe(true);
    expect(caixa('generos', 'RPG').checked).toBe(false);
    expect(botao('Salvar perfil')).toBeTruthy();
  });

  it('exibe e permite remover valores legados fora do catálogo', async () => {
    await carregarCom({
      generos: ['Órfão'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    await abrirEdicao();

    expect(elemento().textContent).toContain('Fora do catálogo (legado)');
    expect(caixa('generos', 'Órfão').checked).toBe(true);

    caixa('generos', 'RPG').click();
    caixa('generos', 'Órfão').click();
    fixture.detectChanges();

    enviar();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.body).toEqual({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
    });

    requisicao.flush({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });
    await aguardarEstabilidade();

    expect(elemento().textContent).not.toContain('Órfão');
  });

  it('bloqueia o salvamento quando o catálogo falha e permite tentar novamente', async () => {
    await carregarCom({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      completo: true,
      atualizadoEm: null,
    });

    botao('Editar perfil').click();
    fixture.detectChanges();

    http
      .expectOne('/api/catalogo/filtros')
      .flush(
        { title: 'Erro', detail: 'Catálogo indisponível.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );
    await aguardarEstabilidade();

    expect(elemento().textContent).toContain('Catálogo indisponível.');
    expect(botao('Salvar perfil').disabled).toBe(true);

    botao('Tentar novamente').click();
    fixture.detectChanges();

    const novaRequisicao = http.expectOne('/api/catalogo/filtros');

    novaRequisicao.flush(FILTROS);
    await aguardarEstabilidade();

    expect(caixa('generos', 'RPG').checked).toBe(true);
    expect(botao('Salvar perfil').disabled).toBe(false);
  });

  it('bloqueia o salvamento quando o catálogo não tem gêneros ou plataformas', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      completo: false,
      atualizadoEm: null,
    });

    await abrirEdicao({ generos: [], plataformas: [], tags: [] });

    expect(elemento().textContent).toContain('ainda não possui opções suficientes');
    expect(botao('Salvar perfil').disabled).toBe(true);

    enviar();
    await aguardarEstabilidade();

    http.expectNone('/api/perfil');
  });

  it('permite tentar novamente quando a carga falha', async () => {
    fixture.detectChanges();

    http
      .expectOne('/api/perfil')
      .flush(
        { title: 'Erro', detail: 'Falha ao carregar o perfil.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );
    await aguardarEstabilidade();

    expect(elemento().textContent).toContain('Falha ao carregar o perfil.');

    botao('Tentar novamente').click();

    const novaRequisicao = http.expectOne('/api/perfil');

    expect(novaRequisicao.request.method).toBe('GET');
    novaRequisicao.flush({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      completo: true,
      atualizadoEm: null,
    });
    await aguardarEstabilidade();

    expect(elemento().textContent).toContain('RPG');
    expect(botao('Editar perfil')).toBeTruthy();
  });
});

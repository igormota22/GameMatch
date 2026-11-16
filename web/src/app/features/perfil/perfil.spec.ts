import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Perfil } from './perfil';

const CHAVE_SESSAO = 'gamematch.sessao';

interface PerfilResposta {
  generos: string[];
  plataformas: string[];
  estilos: string[];
  experiencias: string[];
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

  function textarea(id: string): HTMLTextAreaElement | null {
    return elemento().querySelector(`#${id}`);
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

  function preencher(id: string, valor: string): void {
    const campo = textarea(id);

    if (!campo) {
      throw new Error(`Campo "${id}" não encontrado.`);
    }

    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  function enviar(): void {
    elemento().querySelector('form')?.dispatchEvent(new Event('submit'));
  }

  it('exibe o nome de exibição e as preferências atuais em modo de leitura', async () => {
    await carregarCom({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      experiencias: ['exploração'],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });

    const texto = elemento().textContent;

    expect(texto).toContain('Jogador');
    expect(texto).toContain('Igor');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('PC (Windows)');
    expect(texto).toContain('turn-based');
    expect(texto).toContain('exploração');
    expect(texto).toContain('Perfil completo');
    expect(texto).not.toContain('Perfil incompleto');
    expect(botao('Editar perfil')).toBeTruthy();
    expect(textarea('generos')).toBeNull();
  });

  it('exibe orientação e valores não informados quando o perfil está vazio', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      experiencias: [],
      completo: false,
      atualizadoEm: null,
    });

    const texto = elemento().textContent;

    expect(texto).toContain('Perfil incompleto');
    expect(texto).toContain('Você ainda não informou preferências.');
    expect(texto).toContain('Perfil ainda não salvo');
    expect(texto?.match(/Não informado/g)).toHaveLength(4);
    expect(botao('Editar perfil')).toBeTruthy();
  });

  it('abre a edição com os campos preenchidos a partir do perfil atual', async () => {
    await carregarCom({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      experiencias: [],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });

    botao('Editar perfil').click();
    fixture.detectChanges();

    expect(textarea('generos')?.value).toBe('RPG');
    expect(textarea('plataformas')?.value).toBe('PC (Windows)');
    expect(textarea('estilos')?.value).toBe('turn-based');
    expect(textarea('experiencias')?.value).toBe('');
    expect(botao('Salvar perfil')).toBeTruthy();
  });

  it('volta ao modo de leitura com os valores atualizados ao salvar', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      experiencias: [],
      completo: false,
      atualizadoEm: null,
    });

    botao('Editar perfil').click();
    fixture.detectChanges();

    preencher('generos', '  RPG \nAção\nrpg\n');
    preencher('plataformas', 'PC (Windows)');
    preencher('estilos', 'turn-based');
    preencher('experiencias', '');

    enviar();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      experiencias: [],
    });

    requisicao.flush({
      generos: ['RPG', 'Ação'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      experiencias: [],
      completo: true,
      atualizadoEm: '2026-11-13T12:00:00Z',
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = elemento().textContent;

    expect(texto).toContain('Perfil salvo com sucesso.');
    expect(texto).toContain('RPG, Ação');
    expect(texto).toContain('Perfil completo');
    expect(textarea('generos')).toBeNull();
    expect(botao('Editar perfil')).toBeTruthy();
  });

  it('bloqueia o envio e mostra as mensagens quando os obrigatórios estão vazios', async () => {
    await carregarCom({
      generos: [],
      plataformas: [],
      estilos: [],
      experiencias: [],
      completo: false,
      atualizadoEm: null,
    });

    botao('Editar perfil').click();
    fixture.detectChanges();

    enviar();
    await fixture.whenStable();
    fixture.detectChanges();

    http.expectNone('/api/perfil');
    expect(elemento().textContent).toContain('Informe ao menos um gênero.');
    expect(elemento().textContent).toContain('Informe ao menos uma plataforma.');
    expect(textarea('generos')).not.toBeNull();
  });

  it('permanece em edição e mantém os valores quando o salvamento falha', async () => {
    await carregarCom({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      experiencias: [],
      completo: true,
      atualizadoEm: null,
    });

    botao('Editar perfil').click();
    fixture.detectChanges();

    preencher('generos', 'Estratégia');

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
    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Revise as preferências informadas.');
    expect(elemento().textContent).not.toContain('Perfil salvo com sucesso.');
    expect(textarea('generos')?.value).toBe('Estratégia');
    expect(botao('Salvar perfil')).toBeTruthy();
  });

  it('permite tentar novamente quando a carga falha', async () => {
    fixture.detectChanges();

    http
      .expectOne('/api/perfil')
      .flush(
        { title: 'Erro', detail: 'Falha ao carregar o perfil.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );
    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Falha ao carregar o perfil.');

    botao('Tentar novamente').click();

    const novaRequisicao = http.expectOne('/api/perfil');

    expect(novaRequisicao.request.method).toBe('GET');
    novaRequisicao.flush({
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: [],
      experiencias: [],
      completo: true,
      atualizadoEm: null,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('RPG');
    expect(botao('Editar perfil')).toBeTruthy();
  });
});

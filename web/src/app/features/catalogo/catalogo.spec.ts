import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Catalogo } from './catalogo';

interface ItemCatalogo {
  id: number;
  titulo: string;
  thumbnail: string;
  generos: string[];
  plataformas: string[];
  desenvolvedor: string;
}

interface PaginaResposta {
  items: ItemCatalogo[];
  pagina: number;
  tamanho: number;
  total: number;
}

const FILTROS = {
  generos: ['Racing', 'Strategy'],
  plataformas: ['PC (Windows)'],
  tags: ['action', 'co-op'],
};

function item(id: number, titulo: string): ItemCatalogo {
  return {
    id,
    titulo,
    thumbnail: `https://cdn.example/${id}.jpg`,
    generos: ['Strategy'],
    plataformas: ['PC (Windows)'],
    desenvolvedor: 'Estúdio Aurora',
  };
}

describe('Catalogo', () => {
  let fixture: ComponentFixture<Catalogo>;
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.setItem(
      'gamematch.sessao',
      JSON.stringify({
        token: 'token-de-teste',
        expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
        usuario: { email: 'igor@teste.com', criadoEm: new Date().toISOString() },
      }),
    );

    await TestBed.configureTestingModule({
      imports: [Catalogo],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Catalogo);
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

  async function carregarInicial(pagina: PaginaResposta): Promise<void> {
    fixture.detectChanges();

    http.expectOne('/api/catalogo/filtros').flush(FILTROS);
    http
      .expectOne((requisicao) => requisicao.url === '/api/catalogo')
      .flush(pagina);

    await fixture.whenStable();
    fixture.detectChanges();
  }

  function consultarPagina(pagina: PaginaResposta): void {
    http
      .expectOne((requisicao) => requisicao.url === '/api/catalogo')
      .flush(pagina);
  }

  it('carrega filtros e exibe os cards com imagem, desenvolvedor e atribuição', async () => {
    await carregarInicial({
      items: [item(1, 'Aurora Tactics'), item(2, 'Blue Racer')],
      pagina: 1,
      tamanho: 24,
      total: 2,
    });

    const texto = elemento().textContent ?? '';
    const imagens = Array.from(elemento().querySelectorAll('img'));

    expect(texto).toContain('Aurora Tactics');
    expect(texto).toContain('Blue Racer');
    expect(texto).toContain('Estúdio Aurora');
    expect(texto).toContain('2 jogos encontrados');
    expect(texto).toContain('Página 1 de 1');
    expect(texto).toContain('Dados fornecidos por');
    expect(imagens).toHaveLength(2);
    expect(imagens[0].getAttribute('alt')).toBe('Capa de Aurora Tactics');
    expect(imagens[0].getAttribute('loading')).toBe('lazy');
    expect(imagens[0].getAttribute('src')).toBe('https://cdn.example/1.jpg');

    expect(elemento().querySelector('label[for="busca"]')?.textContent).toContain(
      'Buscar por título',
    );
    expect(elemento().querySelector('label[for="genero"]')).not.toBeNull();
    expect(elemento().querySelector('label[for="plataforma"]')).not.toBeNull();
    expect(elemento().querySelector('label[for="tag"]')).not.toBeNull();
    expect(elemento().querySelector('[aria-live="polite"]')?.textContent).toContain(
      '2 jogos encontrados',
    );

    const genero = elemento().querySelector<HTMLSelectElement>('#genero');
    const opcoes = Array.from(genero?.options ?? []).map((opcao) => opcao.value);

    expect(opcoes).toEqual(['', 'Racing', 'Strategy']);
  });

  it('pesquisa por título com debounce e volta para a primeira página', async () => {
    await carregarInicial({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 24,
      total: 1,
    });

    const busca = elemento().querySelector<HTMLInputElement>('#busca');

    if (!busca) {
      throw new Error('Campo de busca não encontrado.');
    }

    busca.value = 'aurora';
    busca.dispatchEvent(new Event('input'));

    await new Promise((resolver) => setTimeout(resolver, 350));
    fixture.detectChanges();

    const requisicao = http.expectOne(
      (pedido) => pedido.url === '/api/catalogo' &&
        pedido.params.get('busca') === 'aurora' &&
        pedido.params.get('pagina') === '1',
    );

    requisicao.flush({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 24,
      total: 1,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('1 jogo encontrado');
  });

  it('aplica filtros e volta para a primeira página', async () => {
    await carregarInicial({
      items: [item(1, 'Aurora Tactics'), item(2, 'Blue Racer')],
      pagina: 1,
      tamanho: 1,
      total: 2,
    });

    botao('Próxima').click();
    fixture.detectChanges();

    consultarPagina({
      items: [item(2, 'Blue Racer')],
      pagina: 2,
      tamanho: 1,
      total: 2,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Página 2 de 2');

    const genero = elemento().querySelector<HTMLSelectElement>('#genero');

    if (!genero) {
      throw new Error('Filtro de gênero não encontrado.');
    }

    genero.value = 'Strategy';
    genero.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    const requisicao = http.expectOne(
      (pedido) => pedido.url === '/api/catalogo' &&
        pedido.params.get('pagina') === '1' &&
        pedido.params.get('genero') === 'Strategy',
    );

    requisicao.flush({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 1,
      total: 1,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Página 1 de 1');
  });

  it('pagina para frente e para trás atualizando o total', async () => {
    await carregarInicial({
      items: [item(1, 'Aurora Tactics'), item(2, 'Blue Racer')],
      pagina: 1,
      tamanho: 2,
      total: 4,
    });

    botao('Próxima').click();
    fixture.detectChanges();

    consultarPagina({
      items: [item(3, 'Cobalt'), item(4, 'Delta')],
      pagina: 2,
      tamanho: 2,
      total: 4,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Página 2 de 2');
    expect(elemento().textContent).toContain('Cobalt');
    expect(botao('Próxima').disabled).toBe(true);

    botao('Anterior').click();
    fixture.detectChanges();

    consultarPagina({
      items: [item(1, 'Aurora Tactics'), item(2, 'Blue Racer')],
      pagina: 1,
      tamanho: 2,
      total: 4,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Página 1 de 2');
    expect(botao('Anterior').disabled).toBe(true);
  });

  it('distingue catálogo vazio de busca sem resultados', async () => {
    await carregarInicial({ items: [], pagina: 1, tamanho: 24, total: 0 });

    const vazio = elemento().textContent ?? '';

    expect(vazio).toContain('Não há jogos no catálogo');
    expect(vazio).not.toContain('Limpar filtros');
  });

  it('mostra a ação de limpar filtros quando a busca não retorna jogos', async () => {
    await carregarInicial({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 24,
      total: 1,
    });

    const busca = elemento().querySelector<HTMLInputElement>('#busca');

    if (!busca) {
      throw new Error('Campo de busca não encontrado.');
    }

    busca.value = 'inexistente';
    busca.dispatchEvent(new Event('input'));
    await new Promise((resolver) => setTimeout(resolver, 350));
    fixture.detectChanges();

    consultarPagina({ items: [], pagina: 1, tamanho: 24, total: 0 });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Nenhum jogo encontrado');

    botao('Limpar filtros').click();
    fixture.detectChanges();

    const requisicao = http.expectOne(
      (pedido) => pedido.url === '/api/catalogo' && !pedido.params.has('busca'),
    );

    requisicao.flush({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 24,
      total: 1,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().querySelector<HTMLInputElement>('#busca')?.value).toBe('');
    expect(elemento().textContent).toContain('Aurora Tactics');
  });

  it('exibe erro em português e permite tentar novamente', async () => {
    fixture.detectChanges();

    http.expectOne('/api/catalogo/filtros').flush(FILTROS);

    http
      .expectOne((requisicao) => requisicao.url === '/api/catalogo')
      .flush(
        { title: 'Falha', detail: 'Não foi possível carregar o catálogo.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Não foi possível carregar o catálogo.');

    botao('Tentar novamente').click();
    fixture.detectChanges();

    consultarPagina({
      items: [item(1, 'Aurora Tactics')],
      pagina: 1,
      tamanho: 24,
      total: 1,
    });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Aurora Tactics');
  });
});

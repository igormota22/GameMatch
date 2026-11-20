import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Favoritos } from './favoritos';

interface ItemFavorito {
  id: number;
  titulo: string;
  thumbnail: string;
  generos: string[];
  plataformas: string[];
  desenvolvedor: string;
}

interface PaginaFavoritos {
  items: ItemFavorito[];
  pagina: number;
  tamanho: number;
  total: number;
}

function item(id: number, titulo: string): ItemFavorito {
  return {
    id,
    titulo,
    thumbnail: `https://cdn.example/${id}.jpg`,
    generos: ['RPG'],
    plataformas: ['PC (Windows)'],
    desenvolvedor: 'Estúdio Aurora',
  };
}

function pagina(
  items: ItemFavorito[],
  total = items.length,
  numero = 1,
  tamanho = 12,
): PaginaFavoritos {
  return { items, pagina: numero, tamanho, total };
}

describe('Favoritos', () => {
  let fixture: ComponentFixture<Favoritos>;
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [Favoritos],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Favoritos);
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

  function estrela(titulo: string, nota: number): HTMLButtonElement {
    const encontrado = elemento().querySelector<HTMLButtonElement>(
      `[aria-label="Avaliar ${titulo} com ${nota} estrelas"]`,
    );

    if (!encontrado) {
      throw new Error(`Estrela ${nota} de "${titulo}" não encontrada.`);
    }

    return encontrado;
  }

  async function carregarInicial(
    resposta: PaginaFavoritos,
    avaliacoes: Record<number, number> = {},
  ): Promise<void> {
    fixture.detectChanges();

    http
      .expectOne(
        (requisicao) =>
          requisicao.url === '/api/favoritos' && requisicao.method === 'GET',
      )
      .flush(resposta);

    http
      .expectOne('/api/favoritos/ids')
      .flush({ ids: resposta.items.map((jogo) => jogo.id) });

    for (const jogo of resposta.items) {
      const nota = avaliacoes[jogo.id];
      const requisicao = http.expectOne(`/api/avaliacoes/${jogo.id}`);

      if (nota === undefined) {
        requisicao.flush(
          { title: 'Avaliação não encontrada', status: 404 },
          { status: 404, statusText: 'Not Found' },
        );
      } else {
        requisicao.flush({ nota });
      }
    }

    await fixture.whenStable();
    fixture.detectChanges();
  }

  it('lista os favoritos com contagem e paginação', async () => {
    await carregarInicial(pagina([item(1, 'Aurora Tactics')]));

    const texto = elemento().textContent ?? '';

    expect(texto).toContain('Seus jogos marcados');
    expect(texto).toContain('Aurora Tactics');
    expect(texto).toContain('Estúdio Aurora');
    expect(texto).toContain('1 jogo favoritado');
    expect(texto).toContain('Página 1 de 1');
    expect(botao('Anterior').disabled).toBe(true);
    expect(botao('Próxima').disabled).toBe(true);
  });

  it('exibe estado vazio com sugestão de explorar o catálogo', async () => {
    await carregarInicial(pagina([]));

    const texto = elemento().textContent ?? '';

    expect(texto).toContain('Você ainda não tem jogos favoritos');
    expect(elemento().querySelector('a[href="/catalogo"]')?.textContent).toContain(
      'Explorar catálogo',
    );
  });

  it('exibe erro em português e permite tentar novamente', async () => {
    fixture.detectChanges();

    http.expectOne('/api/favoritos/ids').flush({ ids: [] });
    http
      .expectOne((requisicao) => requisicao.url === '/api/favoritos')
      .flush(
        { title: 'Falha', detail: 'Não foi possível carregar os favoritos.', status: 500 },
        { status: 500, statusText: 'Server Error' },
      );

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Não foi possível carregar os favoritos.');

    botao('Tentar novamente').click();
    fixture.detectChanges();

    http
      .expectOne((requisicao) => requisicao.url === '/api/favoritos')
      .flush(pagina([item(1, 'Aurora Tactics')]));

    http.expectOne('/api/avaliacoes/1').flush(
      { title: 'Avaliação não encontrada', status: 404 },
      { status: 404, statusText: 'Not Found' },
    );

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Aurora Tactics');
  });

  it('remove um favorito e recarrega a lista', async () => {
    await carregarInicial(pagina([item(1, 'Aurora Tactics')]));

    const remover = elemento().querySelector<HTMLButtonElement>(
      '[aria-label="Remover Aurora Tactics dos favoritos"]',
    );

    expect(remover).not.toBeNull();

    remover!.click();
    fixture.detectChanges();

    const requisicao = http.expectOne('/api/favoritos/1');

    expect(requisicao.request.method).toBe('DELETE');

    requisicao.flush(null);

    fixture.detectChanges();

    http
      .expectOne((pedido) => pedido.url === '/api/favoritos')
      .flush(pagina([]));

    await fixture.whenStable();
    fixture.detectChanges();

    expect(elemento().textContent).toContain('Você ainda não tem jogos favoritos');
  });

  it('avalia um favorito com estrelas e mantém a nota visível', async () => {
    await carregarInicial(pagina([item(1, 'Aurora Tactics')]));

    expect(estrela('Aurora Tactics', 4).getAttribute('aria-pressed')).toBe('false');

    estrela('Aurora Tactics', 4).click();
    fixture.detectChanges();

    const requisicao = http.expectOne('/api/avaliacoes/1');

    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual({ nota: 4 });

    requisicao.flush({ nota: 4 });

    await fixture.whenStable();
    fixture.detectChanges();

    expect(estrela('Aurora Tactics', 4).getAttribute('aria-pressed')).toBe('true');
    expect(estrela('Aurora Tactics', 3).getAttribute('aria-pressed')).toBe('false');
    expect(elemento().textContent).toContain('Remover avaliação');
  });

  it('remove a avaliação e volta a exibir as estrelas vazias', async () => {
    await carregarInicial(pagina([item(1, 'Aurora Tactics')]), { 1: 3 });

    expect(estrela('Aurora Tactics', 3).getAttribute('aria-pressed')).toBe('true');

    botao('Remover avaliação').click();
    fixture.detectChanges();

    const requisicao = http.expectOne('/api/avaliacoes/1');

    expect(requisicao.request.method).toBe('DELETE');

    requisicao.flush(null);

    await fixture.whenStable();
    fixture.detectChanges();

    expect(estrela('Aurora Tactics', 3).getAttribute('aria-pressed')).toBe('false');
    expect(elemento().textContent).not.toContain('Remover avaliação');
  });
});

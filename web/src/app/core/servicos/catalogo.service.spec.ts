import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CatalogoService } from './catalogo.service';

describe('CatalogoService', () => {
  let servico: CatalogoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    servico = TestBed.inject(CatalogoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('busca o catálogo em /api/catalogo sem parâmetros quando a consulta está vazia', () => {
    servico.listar().subscribe();

    const requisicao = http.expectOne('/api/catalogo');

    expect(requisicao.request.method).toBe('GET');
    expect(requisicao.request.params.keys()).toEqual([]);

    requisicao.flush({ items: [], pagina: 1, tamanho: 24, total: 0 });
  });

  it('envia paginação, busca e filtros como parâmetros da consulta', () => {
    servico
      .listar({
        pagina: 2,
        tamanho: 12,
        busca: 'aurora',
        genero: 'Strategy',
        plataforma: 'PC (Windows)',
        tag: 'action',
      })
      .subscribe();

    const requisicao = http.expectOne((pedido) => pedido.url === '/api/catalogo');
    const parametros = requisicao.request.params;

    expect(requisicao.request.method).toBe('GET');
    expect(parametros.get('pagina')).toBe('2');
    expect(parametros.get('tamanho')).toBe('12');
    expect(parametros.get('busca')).toBe('aurora');
    expect(parametros.get('genero')).toBe('Strategy');
    expect(parametros.get('plataforma')).toBe('PC (Windows)');
    expect(parametros.get('tag')).toBe('action');

    requisicao.flush({ items: [], pagina: 2, tamanho: 12, total: 0 });
  });

  it('omite filtros vazios da consulta', () => {
    servico.listar({ pagina: 1, busca: '   ', genero: '', plataforma: '' }).subscribe();

    const requisicao = http.expectOne((pedido) => pedido.url === '/api/catalogo');

    expect(requisicao.request.params.get('pagina')).toBe('1');
    expect(requisicao.request.params.has('busca')).toBe(false);
    expect(requisicao.request.params.has('genero')).toBe(false);
    expect(requisicao.request.params.has('plataforma')).toBe(false);
    expect(requisicao.request.params.has('tag')).toBe(false);

    requisicao.flush({ items: [], pagina: 1, tamanho: 24, total: 0 });
  });

  it('busca as opções de filtro em /api/catalogo/filtros', () => {
    servico.obterFiltros().subscribe();

    const requisicao = http.expectOne('/api/catalogo/filtros');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush({ generos: [], plataformas: [], tags: [] });
  });
});

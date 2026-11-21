import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { FavoritosService } from './favoritos.service';

describe('FavoritosService', () => {
  let servico: FavoritosService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    servico = TestBed.inject(FavoritosService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('carrega os ids favoritados de /api/favoritos/ids', () => {
    servico.carregar().subscribe();

    const requisicao = http.expectOne('/api/favoritos/ids');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush({ ids: [3, 7] });

    expect(servico.estaFavoritado(3)).toBe(true);
    expect(servico.estaFavoritado(7)).toBe(true);
    expect(servico.estaFavoritado(9)).toBe(false);
  });

  it('não repete a chamada de ids depois de carregado', () => {
    servico.carregar().subscribe();
    http.expectOne('/api/favoritos/ids').flush({ ids: [] });

    servico.carregar().subscribe();

    http.expectNone('/api/favoritos/ids');
  });

  it('favorita de forma otimista com POST e marca o jogo', () => {
    const estados: boolean[] = [];

    servico.alternar(5).subscribe((estado) => estados.push(estado));

    expect(servico.estaFavoritado(5)).toBe(true);

    const requisicao = http.expectOne('/api/favoritos/5');

    expect(requisicao.request.method).toBe('POST');
    expect(servico.estaFavoritado(5)).toBe(true);

    requisicao.flush(null);

    expect(estados).toEqual([true]);
  });

  it('remove o favorito com DELETE e desmarca o jogo', () => {
    servico.carregar().subscribe();
    http.expectOne('/api/favoritos/ids').flush({ ids: [5] });

    servico.alternar(5).subscribe();

    expect(servico.estaFavoritado(5)).toBe(false);

    const requisicao = http.expectOne('/api/favoritos/5');

    expect(requisicao.request.method).toBe('DELETE');

    requisicao.flush(null);

    expect(servico.estaFavoritado(5)).toBe(false);
  });

  it('reverte o estado otimista quando a API falha', () => {
    let sucesso = false;
    let erroRecebido = false;

    servico.alternar(8).subscribe({
      next: () => (sucesso = true),
      error: () => (erroRecebido = true),
    });

    expect(servico.estaFavoritado(8)).toBe(true);

    http.expectOne('/api/favoritos/8').flush('erro', {
      status: 500,
      statusText: 'Server Error',
    });

    expect(erroRecebido).toBe(true);
    expect(sucesso).toBe(false);
    expect(servico.estaFavoritado(8)).toBe(false);
  });

  it('lista os favoritos com a paginação informada', () => {
    servico.listar(2, 6).subscribe();

    const requisicao = http.expectOne(
      (pedido) => pedido.url === '/api/favoritos',
    );

    expect(requisicao.request.method).toBe('GET');
    expect(requisicao.request.params.get('pagina')).toBe('2');
    expect(requisicao.request.params.get('tamanho')).toBe('6');

    requisicao.flush({ items: [], pagina: 2, tamanho: 6, total: 0 });
  });

  it('remove com DELETE e restaura o estado quando a API falha', () => {
    servico.carregar().subscribe();
    http.expectOne('/api/favoritos/ids').flush({ ids: [4] });

    let erroRecebido = false;

    servico.remover(4).subscribe({
      next: () => undefined,
      error: () => (erroRecebido = true),
    });

    expect(servico.estaFavoritado(4)).toBe(false);

    http.expectOne('/api/favoritos/4').flush('erro', {
      status: 500,
      statusText: 'Server Error',
    });

    expect(erroRecebido).toBe(true);
    expect(servico.estaFavoritado(4)).toBe(true);
  });

  it('limpa o estado e recarrega os ids na próxima sessão', () => {
    servico.carregar().subscribe();
    http.expectOne('/api/favoritos/ids').flush({ ids: [1] });

    servico.limpar();

    expect(servico.estaFavoritado(1)).toBe(false);

    servico.carregar().subscribe();
    http.expectOne('/api/favoritos/ids').flush({ ids: [] });
  });
});

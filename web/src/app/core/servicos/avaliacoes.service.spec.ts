import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AvaliacoesService } from './avaliacoes.service';

describe('AvaliacoesService', () => {
  let servico: AvaliacoesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    servico = TestBed.inject(AvaliacoesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('consulta a avaliação do jogo em /api/avaliacoes/{id}', () => {
    servico.obter(7).subscribe();

    const requisicao = http.expectOne('/api/avaliacoes/7');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush({ nota: 4 });
  });

  it('envia a nota com PUT para /api/avaliacoes/{id}', () => {
    servico.salvar(7, 5).subscribe();

    const requisicao = http.expectOne('/api/avaliacoes/7');

    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual({ nota: 5 });

    requisicao.flush({ nota: 5 });
  });

  it('remove a avaliação com DELETE em /api/avaliacoes/{id}', () => {
    servico.remover(7).subscribe();

    const requisicao = http.expectOne('/api/avaliacoes/7');

    expect(requisicao.request.method).toBe('DELETE');

    requisicao.flush(null);
  });

  it('propaga o erro da API ao consultar', () => {
    let erroRecebido = false;

    servico.obter(7).subscribe({
      next: () => undefined,
      error: () => (erroRecebido = true),
    });

    http.expectOne('/api/avaliacoes/7').flush('erro', {
      status: 500,
      statusText: 'Server Error',
    });

    expect(erroRecebido).toBe(true);
  });
});

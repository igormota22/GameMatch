import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { autorizacaoInterceptor } from './autorizacao.interceptor';
import { SessaoService } from './sessao.service';

describe('autorizacaoInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([autorizacaoInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('adiciona o cabeçalho Authorization nas chamadas da API', () => {
    TestBed.inject(SessaoService).iniciar({
      token: 'token-teste',
      expiraEm: new Date(Date.now() + 60_000).toISOString(),
      usuario: { email: 'jogador@teste.com', criadoEm: new Date().toISOString() },
    });

    http.get('/api/conta').subscribe();
    const requisicao = httpTesting.expectOne('/api/conta');

    expect(requisicao.request.headers.get('Authorization')).toBe('Bearer token-teste');

    requisicao.flush({});
  });

  it('não adiciona o cabeçalho quando não há sessão', () => {
    http.get('/api/conta').subscribe();
    const requisicao = httpTesting.expectOne('/api/conta');

    expect(requisicao.request.headers.has('Authorization')).toBe(false);

    requisicao.flush({});
  });

  it('encerra a sessão quando a API responde 401', () => {
    const sessaoService = TestBed.inject(SessaoService);
    sessaoService.iniciar({
      token: 'token-teste',
      expiraEm: new Date(Date.now() + 60_000).toISOString(),
      usuario: { email: 'jogador@teste.com', criadoEm: new Date().toISOString() },
    });

    http.get('/api/conta').subscribe({ error: () => undefined });
    httpTesting
      .expectOne('/api/conta')
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(sessaoService.autenticado()).toBe(false);
  });
});

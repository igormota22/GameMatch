import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PerfilService } from './perfil.service';

describe('PerfilService', () => {
  let servico: PerfilService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    servico = TestBed.inject(PerfilService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('busca o perfil autenticado em /api/perfil', () => {
    servico.obter().subscribe();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.method).toBe('GET');

    requisicao.flush({
      generos: [],
      plataformas: [],
      estilos: [],
      experiencias: [],
      completo: false,
      atualizadoEm: null,
    });
  });

  it('envia o perfil com PUT para /api/perfil', () => {
    const perfil = {
      generos: ['RPG'],
      plataformas: ['PC (Windows)'],
      estilos: ['turn-based'],
      experiencias: [],
    };

    servico.salvar(perfil).subscribe();

    const requisicao = http.expectOne('/api/perfil');

    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual(perfil);

    requisicao.flush({
      ...perfil,
      completo: true,
      atualizadoEm: new Date().toISOString(),
    });
  });
});

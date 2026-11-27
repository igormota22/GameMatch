import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { CriarConta } from './criar-conta';

const CHAVE_SESSAO = 'gamematch.sessao';

const RESPOSTA_CADASTRO = {
  token: 'token-de-teste',
  expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
  usuario: { email: 'novo@teste.com', criadoEm: new Date().toISOString() },
};

describe('CriarConta', () => {
  let fixture: ComponentFixture<CriarConta>;
  let http: HttpTestingController;
  let navegar: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    localStorage.removeItem(CHAVE_SESSAO);

    await TestBed.configureTestingModule({
      imports: [CriarConta],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CriarConta);
    http = TestBed.inject(HttpTestingController);

    const router = TestBed.inject(Router);
    navegar = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
    localStorage.removeItem(CHAVE_SESSAO);
  });

  function elemento(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function preencher(campo: string, valor: string): void {
    const entrada = elemento().querySelector<HTMLInputElement>(`input[formControlName="${campo}"]`);

    if (!entrada) {
      throw new Error(`Campo "${campo}" não encontrado.`);
    }

    entrada.value = valor;
    entrada.dispatchEvent(new Event('input'));
  }

  function enviar(): void {
    elemento().querySelector('form')?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('autentica e redireciona para o perfil após um cadastro válido', async () => {
    preencher('email', 'novo@teste.com');
    preencher('senha', 'SenhaForte1');

    enviar();

    const requisicao = http.expectOne('/api/contas');

    expect(requisicao.request.method).toBe('POST');
    expect(requisicao.request.body).toEqual({ email: 'novo@teste.com', senha: 'SenhaForte1' });

    requisicao.flush(RESPOSTA_CADASTRO);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(navegar).toHaveBeenCalledWith('/perfil');
    expect(localStorage.getItem(CHAVE_SESSAO)).toContain('token-de-teste');
  });

  it('não envia requisição quando o formulário é inválido', () => {
    preencher('email', 'email-invalido');
    preencher('senha', '123');

    enviar();

    http.expectNone('/api/contas');
    expect(navegar).not.toHaveBeenCalled();
    expect(elemento().textContent).toContain('Informe um e-mail válido.');
    expect(elemento().textContent).toContain('A senha precisa ter pelo menos 8 caracteres.');
  });
});

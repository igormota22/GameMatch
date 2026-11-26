import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NaoEncontrado } from './nao-encontrado';

describe('NaoEncontrado', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NaoEncontrado],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  function criar(): HTMLElement {
    const fixture = TestBed.createComponent(NaoEncontrado);

    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('exibe o título e a orientação da página 404', () => {
    const elemento = criar();

    expect(elemento.querySelector('h1')?.textContent?.trim()).toBe('Página não encontrada');
    expect(elemento.textContent).toContain('Erro 404');
    expect(elemento.textContent).toContain('O endereço acessado não existe ou foi movido.');
  });

  it('oferece o retorno para o início', () => {
    const elemento = criar();
    const link = elemento.querySelector('a');

    expect(link?.textContent?.trim()).toBe('Voltar para o início');
    expect(link?.getAttribute('href')).toBe('/');
  });
});

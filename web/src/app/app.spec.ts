import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render header, main content and footer in Portuguese', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const elemento = fixture.nativeElement as HTMLElement;

    expect(elemento.querySelector('app-cabecalho')?.textContent).toContain('GameMatch');
    expect(elemento.querySelector('main')).toBeTruthy();
    expect(elemento.querySelector('app-rodape')?.textContent).toContain(
      'Dados fornecidos por FreeToGame.com',
    );
  });

  it('should render the FreeToGame attribution link safely', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const link = (fixture.nativeElement as HTMLElement).querySelector('app-rodape a');

    expect(link?.getAttribute('href')).toBe('https://www.freetogame.com');
    expect(link?.getAttribute('target')).toBe('_blank');
    expect(link?.getAttribute('rel')).toBe('noopener noreferrer');
  });
});

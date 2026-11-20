import { Routes } from '@angular/router';
import { autenticadoGuard } from './core/sessao/autenticado.guard';

export const routes: Routes = [
  {
    path: '',
    title: 'GameMatch — Início',
    loadComponent: () => import('./features/inicio/inicio').then((modulo) => modulo.Inicio),
  },
  {
    path: 'entrar',
    title: 'GameMatch — Entrar',
    loadComponent: () => import('./features/entrar/entrar').then((modulo) => modulo.Entrar),
  },
  {
    path: 'criar-conta',
    title: 'GameMatch — Criar conta',
    loadComponent: () =>
      import('./features/criar-conta/criar-conta').then((modulo) => modulo.CriarConta),
  },
  {
    path: 'conta',
    title: 'GameMatch — Minha conta',
    canActivate: [autenticadoGuard],
    loadComponent: () => import('./features/conta/conta').then((modulo) => modulo.Conta),
  },
  {
    path: 'perfil',
    title: 'GameMatch — Perfil gamer',
    canActivate: [autenticadoGuard],
    loadComponent: () => import('./features/perfil/perfil').then((modulo) => modulo.Perfil),
  },
  {
    path: 'catalogo',
    title: 'GameMatch — Catálogo',
    canActivate: [autenticadoGuard],
    loadComponent: () =>
      import('./features/catalogo/catalogo').then((modulo) => modulo.Catalogo),
  },
  {
    path: 'recomendacoes',
    title: 'GameMatch — Recomendações',
    canActivate: [autenticadoGuard],
    loadComponent: () =>
      import('./features/recomendacoes/recomendacoes').then((modulo) => modulo.Recomendacoes),
  },
  {
    path: 'favoritos',
    title: 'GameMatch — Favoritos',
    canActivate: [autenticadoGuard],
    loadComponent: () =>
      import('./features/favoritos/favoritos').then((modulo) => modulo.Favoritos),
  },
  {
    path: '**',
    title: 'GameMatch — Página não encontrada',
    loadComponent: () =>
      import('./features/nao-encontrado/nao-encontrado').then((modulo) => modulo.NaoEncontrado),
  },
];

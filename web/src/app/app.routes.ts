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
    path: '**',
    title: 'GameMatch — Página não encontrada',
    loadComponent: () =>
      import('./features/nao-encontrado/nao-encontrado').then((modulo) => modulo.NaoEncontrado),
  },
];

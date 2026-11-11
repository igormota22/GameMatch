import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'GameMatch — Início',
    loadComponent: () => import('./features/inicio/inicio').then((modulo) => modulo.Inicio),
  },
  {
    path: '**',
    title: 'GameMatch — Página não encontrada',
    loadComponent: () =>
      import('./features/nao-encontrado/nao-encontrado').then((modulo) => modulo.NaoEncontrado),
  },
];

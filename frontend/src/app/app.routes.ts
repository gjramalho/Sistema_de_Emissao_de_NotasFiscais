import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/produtos/produtos-list/produtos-list.component').then(m => m.ProdutosListComponent)
  },
  {
    path: 'notas-fiscais',
    loadComponent: () =>
      import('./features/notas-fiscais/notas-list/nota-list.component').then(m => m.NotasListComponent)
  },
  {
    path: 'notas-fiscais/nova',
    loadComponent: () =>
      import('./features/notas-fiscais/nota-form/nota-form.component').then(m => m.NotaFormComponent)
  },
  {
    path: 'notas-fiscais/:id',
    loadComponent: () =>
      import('./features/notas-fiscais/nota-detail/nota-detail.component').then(m => m.NotaDetailComponent)
  },
  { path: '**', redirectTo: 'produtos' }
];


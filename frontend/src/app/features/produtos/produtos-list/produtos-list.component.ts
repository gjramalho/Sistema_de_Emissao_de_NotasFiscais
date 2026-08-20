import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { Observable, Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { Produto } from '../../../core/models/produto.model';
import { ProdutoService } from '../../../core/services/produto.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProdutoFormComponent } from '../produto-form/produto-form.component';

@Component({
  selector: 'app-produtos-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    ProdutoFormComponent
  ],
  templateUrl: './produtos-list.component.html',
  styleUrl: './produtos-list.component.scss'
})
export class ProdutosListComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly colunas = ['codigo', 'descricao', 'saldo', 'acoes'];
  readonly busca = new FormControl('', { nonNullable: true });

  produtos!: Observable<Produto[]>;
  termoBusca = '';

  constructor(
    private readonly produtoService: ProdutoService,
    private readonly notification: NotificationService
  ) {
    this.produtos = this.produtoService.produtos$;
  }

  ngOnInit(): void {
    this.produtoService.carregar().subscribe();

    this.busca.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(termo => (this.termoBusca = termo.trim().toLowerCase()));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onProdutoCriado(): void {
  }

  remover(id: number, codigo: string): void {
    if (!confirm(`Remover o produto ${codigo}?`)) return;

    this.produtoService.remover(id).subscribe({
      next: () => this.notification.sucesso(`Produto ${codigo} removido.`)
    });
  }
}

import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BehaviorSubject, Observable, Subject, finalize, takeUntil } from 'rxjs';

import { ProdutoService } from '../../../core/services/produto.service';
import { NotaFiscalService } from '../../../core/services/nota-fiscal.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ItemNotaFiscal } from '../../../core/models/nota-fiscal.model';
import { Produto } from '../../../core/models/produto.model';


@Component({
  selector: 'app-nota-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule
  ],
  templateUrl: './nota-form.component.html',
  styleUrl: './nota-form.component.scss'
})
export class NotaFormComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly itensSubject = new BehaviorSubject<ItemNotaFiscal[]>([]);

  readonly itens$ = this.itensSubject.asObservable();
  readonly colunasItens = ['descricao', 'quantidade', 'acoes'];

  produtos!: Observable<Produto[]>;
  salvando = false;

  itemForm!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly produtoService: ProdutoService,
    private readonly notaFiscalService: NotaFiscalService,
    private readonly notification: NotificationService,
    private readonly router: Router
  ) {

    this.produtos = this.produtoService.produtos$;
    this.itemForm = this.fb.group({
      produtoId: [null as number | null, Validators.required],
      quantidade: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.produtoService.carregar().pipe(takeUntil(this.destroy$)).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  adicionarItem(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    const { produtoId, quantidade } = this.itemForm.getRawValue();

    // Busca o produto selecionado a partir do último snapshot carregado.
    // Usamos take(1) implicitamente ao dar unsubscribe logo após o primeiro valor.
    this.produtoService.produtos$.pipe(takeUntil(this.destroy$)).subscribe(produtos => {
      const produto = produtos.find(p => p.id === produtoId);
      if (!produto) return;

      const itensAtuais = this.itensSubject.value;
      const jaExiste = itensAtuais.find(i => i.produtoId === produto.id);

      if (jaExiste) {
        jaExiste.quantidade += quantidade!;
        this.itensSubject.next([...itensAtuais]);
      } else {
        this.itensSubject.next([
          ...itensAtuais,
          { produtoId: produto.id, descricaoProduto: produto.descricao, quantidade: quantidade! }
        ]);
      }

      this.itemForm.reset({ produtoId: null, quantidade: 1 });
    }).unsubscribe();
  }

  removerItem(produtoId: number): void {
    this.itensSubject.next(this.itensSubject.value.filter(i => i.produtoId !== produtoId));
  }

  salvarNota(): void {
    const itens = this.itensSubject.value;
    if (itens.length === 0) {
      this.notification.erro('Adicione ao menos um produto à nota fiscal.');
      return;
    }

    this.salvando = true;
    this.notaFiscalService
      .criar({ itens })
      .pipe(finalize(() => (this.salvando = false)))
      .subscribe({
        next: nota => {
          this.notification.sucesso(`Nota fiscal #${nota.numero} criada com status Aberta.`);
          this.router.navigate(['/notas-fiscais', nota.id]);
        }
      });
  }
}

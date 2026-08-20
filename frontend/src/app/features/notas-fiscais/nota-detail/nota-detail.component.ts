import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Subject, finalize, switchMap, takeUntil } from 'rxjs';

import { NotaFiscalService } from '../../../core/services/nota-fiscal.service';
import { NotificationService } from '../../../core/services/notification.service';
import { DebugService } from '../../../core/services/debug.service';
import { NotaFiscal } from '../../../core/models/nota-fiscal.model';

@Component({
  selector: 'app-nota-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule
  ],
  templateUrl: './nota-detail.component.html',
  styleUrl: './nota-detail.component.scss'
})
export class NotaDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly colunas = ['descricao', 'quantidade'];

  nota: NotaFiscal | null = null;
  imprimindo = false;
  simulandoFalha = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly notaFiscalService: NotaFiscalService,
    private readonly debugService: DebugService,
    private readonly notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        switchMap(params => this.notaFiscalService.obterPorId(Number(params.get('id')))),
        takeUntil(this.destroy$)
      )
      .subscribe(nota => (this.nota = nota));

    this.debugService.status().pipe(takeUntil(this.destroy$))
      .subscribe(s => (this.simulandoFalha = s.simulandoFalha));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  imprimir(): void {
    if (!this.nota) return;

    this.imprimindo = true;
    this.notaFiscalService
      .imprimir(this.nota.id)
      .pipe(finalize(() => (this.imprimindo = false)))
      .subscribe({
        next: resultado => {
          if (resultado.nota) this.nota = resultado.nota;
          if (resultado.sucesso) {
            this.imprimindo = false;
            setTimeout(() => window.print());
          } else {
            this.notification.erro(resultado.mensagem);
          }
        },
        error: (err) => {
          const nota = err?.error?.nota;
          if (nota) this.nota = nota;
        }
      });
  }

  alternarFalhaSimulada(ligar: boolean): void {
    const acao$ = ligar ? this.debugService.ativarFalha() : this.debugService.restaurar();
    acao$.subscribe(() => {
      this.simulandoFalha = ligar;
      this.notification.sucesso(
        ligar ? 'Falha simulada ATIVADA no Serviço de Estoque.' : 'Serviço de Estoque restaurado.'
      );
    });
  }
}
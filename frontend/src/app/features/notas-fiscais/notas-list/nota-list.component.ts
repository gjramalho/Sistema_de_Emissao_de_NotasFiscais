import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { Observable } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

import { NotaFiscal } from '../../../core/models/nota-fiscal.model';
import { NotaFiscalService } from '../../../core/services/nota-fiscal.service';

@Component({
  selector: 'app-notas-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './nota-list.component.html',
  styleUrl: './nota-list.component.scss'
})
export class NotasListComponent implements OnInit {
  readonly colunas = ['numero', 'status', 'itens', 'criadaEm', 'acoes'];
  notas!: Observable<NotaFiscal[]>;

  constructor(private readonly notaFiscalService: NotaFiscalService) {
    this.notas = this.notaFiscalService.notas$;
  }

  // Busca as notas assim que a tela é criada.
  ngOnInit(): void {
    this.notaFiscalService.carregar().subscribe();
  }
}

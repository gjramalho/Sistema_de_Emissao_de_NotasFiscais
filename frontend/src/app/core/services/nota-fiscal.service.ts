import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environments';
import { CriarNotaFiscalPayload, ImprimirNotaResultado, NotaFiscal } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private readonly baseUrl = `${environment.faturamentoApiUrl}/notas-fiscais`;

  private readonly notasSubject = new BehaviorSubject<NotaFiscal[]>([]);
  readonly notas$: Observable<NotaFiscal[]> = this.notasSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  carregar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.baseUrl).pipe(
      tap(notas => this.notasSubject.next(notas))
    );
  }

  obterPorId(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.baseUrl}/${id}`);
  }

  criar(payload: CriarNotaFiscalPayload): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, payload);
  }

  imprimir(id: number): Observable<ImprimirNotaResultado> {
    return this.http.post<ImprimirNotaResultado>(`${this.baseUrl}/${id}/imprimir`, {});
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environments';
import { AtualizarProdutoPayload, CriarProdutoPayload, Produto } from '../models/produto.model';


@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly baseUrl = `${environment.estoqueApiUrl}/produtos`;

  private readonly produtosSubject = new BehaviorSubject<Produto[]>([]);
 
  readonly produtos$: Observable<Produto[]> = this.produtosSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  carregar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl).pipe(
      tap(produtos => this.produtosSubject.next(produtos))
    );
  }

  obterPorId(id: number): Observable<Produto> {
    return this.http.get<Produto>(`${this.baseUrl}/${id}`);
  }

  criar(payload: CriarProdutoPayload): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, payload).pipe(
      tap(() => this.carregar().subscribe())
    );
  }

  atualizar(id: number, payload: AtualizarProdutoPayload): Observable<Produto> {
    return this.http.put<Produto>(`${this.baseUrl}/${id}`, payload).pipe(
      tap(() => this.carregar().subscribe())
    );
  }

  remover(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.carregar().subscribe())
    );
  }
}

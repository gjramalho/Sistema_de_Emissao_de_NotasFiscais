import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';

@Injectable({ providedIn: 'root' })
export class DebugService {
  private readonly baseUrl = `${environment.estoqueApiUrl}/debug`;

  constructor(private readonly http: HttpClient) {}

  status(): Observable<{ simulandoFalha: boolean }> {
    return this.http.get<{ simulandoFalha: boolean }>(`${this.baseUrl}/status`);
  }

  ativarFalha(): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/falhar`, {});
  }

  restaurar(): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/restaurar`, {});
  }
}

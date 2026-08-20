import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';


export const errorInterceptor: HttpInterceptorFn = (req, next) => {

  const notification = inject(NotificationService);
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const mensagemBackend =
        error.error?.mensagem ?? error.error?.detalhe ?? 'Erro inesperado de comunicação com o servidor.';

      const mensagem =
        error.status === 0
          ? 'Não foi possível conectar ao servidor. Verifique se os serviços de backend estão em execução.'
          : mensagemBackend;

      notification.erro(mensagem);
      return throwError(() => error);
    })
  );
};

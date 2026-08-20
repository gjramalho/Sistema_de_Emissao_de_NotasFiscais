import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.componente';

// Inicializa o componente raiz com rotas, HTTP, animações e interceptor configurados.
bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));

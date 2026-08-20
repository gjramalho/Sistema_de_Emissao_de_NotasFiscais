import { Component, EventEmitter, Output } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs';
import { ProdutoService } from '../../../core/services/produto.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-produto-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './produto-form.component.html',
  styleUrl: './produto-form.component.scss'
})
export class ProdutoFormComponent {
  @Output() produtoCriado = new EventEmitter<void>();

  salvando = false;
  form!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly produtoService: ProdutoService,
    private readonly notification: NotificationService
  ) {
    this.form = this.fb.group({
      codigo: ['', [Validators.required, Validators.maxLength(50)]],
      descricao: ['', [Validators.required, Validators.maxLength(200)]],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.salvando = true;
    const valor = this.form.getRawValue();

    this.produtoService
      .criar({ codigo: valor.codigo!, descricao: valor.descricao!, saldo: valor.saldo! })
      .pipe(finalize(() => (this.salvando = false)))
      .subscribe({
        next: () => {
          this.notification.sucesso(`Produto "${valor.codigo}" cadastrado com sucesso.`);
          this.form.reset({ codigo: '', descricao: '', saldo: 0 });
          this.produtoCriado.emit();
        }
      });
  }
}
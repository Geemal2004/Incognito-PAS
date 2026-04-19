import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  template: `
    <div class="confirm-dialog">
      <div class="confirm-dialog__icon">
        <mat-icon>warning_amber</mat-icon>
      </div>
      <div class="confirm-dialog__copy">
        <h2>{{ data.title }}</h2>
        <p>{{ data.message }}</p>
      </div>
    </div>

    <div class="dialog-actions">
      <button mat-stroked-button class="btn-secondary" (click)="close(false)">Cancel</button>
      <button mat-flat-button class="btn-danger" (click)="close(true)">{{ data.confirmText || 'Confirm' }}</button>
    </div>
  `,
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialogComponent {
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);

  close(result: boolean): void {
    this.dialogRef.close(result);
  }
}

import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3000, panelClass: ['toast-success'] });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Dismiss', { duration: 5000, panelClass: ['toast-error'] });
  }

  info(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3500, panelClass: ['toast-info'] });
  }
}

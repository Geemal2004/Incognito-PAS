import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ResearchAreaDto } from '../../core/models/api.models';
import { ModuleLeaderApiService } from '../../core/services/module-leader-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

@Component({
  selector: 'app-module-leader-research-areas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './module-leader-research-areas.component.html',
  styleUrl: './module-leader-research-areas.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModuleLeaderResearchAreasComponent {
  private readonly api = inject(ModuleLeaderApiService);
  private readonly fb = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly areas = signal<ResearchAreaDto[]>([]);
  readonly editingId = signal<number | null>(null);

  readonly createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    description: ['', [Validators.maxLength(500)]]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getResearchAreas().subscribe({
      next: (areas) => {
        this.areas.set(areas);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load research areas.');
      }
    });
  }

  create(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const payload = this.createForm.getRawValue();
    this.api.createResearchArea(payload).subscribe({
      next: () => {
        this.toast.success('Research area created.');
        this.createForm.reset({ name: '', description: '' });
        this.load();
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to create research area.')
    });
  }

  startEdit(area: ResearchAreaDto): void {
    this.editingId.set(area.id);
    this.editForm.setValue({
      name: area.name,
      description: area.description ?? ''
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(): void {
    const id = this.editingId();
    if (id === null) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const payload = this.editForm.getRawValue();
    this.api.updateResearchArea(id, { ...payload, isActive: true }).subscribe({
      next: () => {
        this.toast.success('Research area updated.');
        this.editingId.set(null);
        this.load();
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to update research area.')
    });
  }

  deactivate(id: number): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Deactivate research area',
        message: 'This will disable the area for future selections. Continue?',
        confirmText: 'Deactivate'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.api.deleteResearchArea(id).subscribe({
        next: () => {
          this.toast.success('Research area deactivated.');
          this.load();
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to deactivate research area.')
      });
    });
  }
}

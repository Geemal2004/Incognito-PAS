import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { AdminApiService } from '../../core/services/admin-api.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

@Component({
  selector: 'app-admin-migrations',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule, PageHeaderComponent, LoadingSkeletonComponent, EmptyStateComponent],
  templateUrl: './admin-migrations.component.html',
  styleUrl: './admin-migrations.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminMigrationsComponent {
  private readonly api = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly migrations = signal<string[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getMigrations().subscribe({
      next: (result) => {
        this.migrations.set(result.migrations);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load migration status.');
      }
    });
  }
}

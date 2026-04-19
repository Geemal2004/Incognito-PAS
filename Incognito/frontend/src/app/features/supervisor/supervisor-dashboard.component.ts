import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { SupervisorDashboardDto } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { StatCardComponent } from '../../shared/ui/stat-card.component';

@Component({
  standalone: true,
  selector: 'app-supervisor-dashboard',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    PageHeaderComponent,
    StatCardComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './supervisor-dashboard.component.html',
  styleUrl: './supervisor-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorDashboardComponent {
  private readonly api = inject(SupervisorApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly dashboard = signal<SupervisorDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load supervisor dashboard.');
      }
    });
  }
}

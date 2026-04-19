import { DatePipe } from '@angular/common';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { StudentApiService } from '../../core/services/student-api.service';
import { StudentDashboardDto } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { StatCardComponent } from '../../shared/ui/stat-card.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge.component';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    RouterLink,
    MatButtonModule,
    PageHeaderComponent,
    StatCardComponent,
    StatusBadgeComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentDashboardComponent {
  private readonly api = inject(StudentApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly dashboard = signal<StudentDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load student dashboard.');
      }
    });
  }
}

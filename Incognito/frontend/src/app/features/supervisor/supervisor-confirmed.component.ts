import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatchSummaryDto } from '../../core/models/api.models';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

@Component({
  selector: 'app-supervisor-confirmed',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, MatCardModule, PageHeaderComponent, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './supervisor-confirmed.component.html',
  styleUrl: './supervisor-confirmed.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorConfirmedComponent {
  private readonly api = inject(SupervisorApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly confirmed = signal<MatchSummaryDto[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getMatches().subscribe({
      next: (data) => {
        this.confirmed.set(data.confirmed);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load confirmed matches.');
      }
    });
  }
}

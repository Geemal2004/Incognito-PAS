import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatchSummaryDto } from '../../core/models/api.models';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { ProposalCardComponent } from '../../shared/ui/proposal-card.component';

@Component({
  selector: 'app-supervisor-interests',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatDialogModule,
    PageHeaderComponent,
    ProposalCardComponent,
    EmptyStateComponent,
    LoadingSkeletonComponent
  ],
  templateUrl: './supervisor-interests.component.html',
  styleUrl: './supervisor-interests.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorInterestsComponent {
  private readonly api = inject(SupervisorApiService);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly interests = signal<MatchSummaryDto[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getMatches().subscribe({
      next: (data) => {
        this.interests.set(data.interests);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load interests.');
      }
    });
  }

  confirm(matchId: number): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm match',
        message: 'Confirming reveals identities to both parties and finalizes this match.',
        confirmText: 'Confirm match'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.api.confirmMatch(matchId).subscribe({
        next: () => {
          this.toast.success('Match confirmed. Identity has been revealed.');
          this.load();
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to confirm match.')
      });
    });
  }
}

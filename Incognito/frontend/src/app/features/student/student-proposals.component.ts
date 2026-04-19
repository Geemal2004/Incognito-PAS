import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { StudentApiService } from '../../core/services/student-api.service';
import { ProposalListItemDto } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { ProposalCardComponent } from '../../shared/ui/proposal-card.component';

@Component({
  selector: 'app-student-proposals',
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
  templateUrl: './student-proposals.component.html',
  styleUrl: './student-proposals.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentProposalsComponent {
  private readonly api = inject(StudentApiService);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly proposals = signal<ProposalListItemDto[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getProposals().subscribe({
      next: (items) => {
        this.proposals.set(items);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load proposals.');
      }
    });
  }

  canEdit(proposal: ProposalListItemDto): boolean {
    return proposal.status.toLowerCase() === 'pending' && proposal.canManage !== false;
  }

  canWithdraw(proposal: ProposalListItemDto): boolean {
    const value = proposal.status.toLowerCase();
    return value !== 'matched' && value !== 'withdrawn' && proposal.canManage !== false;
  }

  withdraw(id: number): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Withdraw proposal',
        message: 'This will remove your proposal from active matching. Continue?',
        confirmText: 'Withdraw'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.api.withdrawProposal(id).subscribe({
        next: () => {
          this.toast.success('Proposal withdrawn successfully.');
          this.load();
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to withdraw proposal.')
      });
    });
  }
}

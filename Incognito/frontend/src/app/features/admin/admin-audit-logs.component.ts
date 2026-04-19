import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AdminApiService } from '../../core/services/admin-api.service';
import { AuditLogDto, PagedResult } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { AuditTimelineComponent } from '../../shared/ui/audit-timeline.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { FilterBarComponent } from '../../shared/ui/filter-bar.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { SearchBoxComponent } from '../../shared/ui/search-box.component';

@Component({
  selector: 'app-admin-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatButtonModule,
    PageHeaderComponent,
    FilterBarComponent,
    SearchBoxComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    AuditTimelineComponent
  ],
  templateUrl: './admin-audit-logs.component.html',
  styleUrl: './admin-audit-logs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminAuditLogsComponent {
  private readonly api = inject(AdminApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly page = signal(1);
  readonly pageSize = signal(25);
  readonly entityType = signal('');
  readonly action = signal('');
  readonly result = signal<PagedResult<AuditLogDto> | null>(null);

  constructor() {
    this.load();
  }

  updateEntityType(value: string): void {
    this.entityType.set(value);
    this.page.set(1);
    this.load();
  }

  updateAction(value: string): void {
    this.action.set(value);
    this.page.set(1);
    this.load();
  }

  previousPage(): void {
    if (this.page() <= 1) {
      return;
    }

    this.page.set(this.page() - 1);
    this.load();
  }

  nextPage(): void {
    if (!this.hasNextPage()) {
      return;
    }

    this.page.set(this.page() + 1);
    this.load();
  }

  hasNextPage(): boolean {
    const value = this.result();
    if (!value) {
      return false;
    }

    return value.page * value.pageSize < value.total;
  }

  totalPages(): number {
    const value = this.result();
    if (!value || value.pageSize <= 0) {
      return 1;
    }

    return Math.max(1, Math.ceil(value.total / value.pageSize));
  }

  load(): void {
    this.loading.set(true);
    this.api.getAuditLogs({
      page: this.page(),
      pageSize: this.pageSize(),
      entityType: this.entityType() || undefined,
      action: this.action() || undefined
    }).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load audit logs.');
      }
    });
  }
}

import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { AdminApiService } from '../../core/services/admin-api.service';
import { AdminDashboardDto } from '../../core/models/api.models';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { StatCardComponent } from '../../shared/ui/stat-card.component';

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, PageHeaderComponent, StatCardComponent],
  template: `
    <app-page-header title="System Administration" subtitle="Govern users, migration health, and audit evidence."></app-page-header>

    <section class="stats-grid" *ngIf="dashboard() as d">
      <app-stat-card label="Total Users" [value]="d.totalUsers"></app-stat-card>
      <app-stat-card label="Total Proposals" [value]="d.totalProposals"></app-stat-card>
      <app-stat-card label="Pending Proposals" [value]="d.pendingProposals"></app-stat-card>
      <app-stat-card label="Confirmed Matches" [value]="d.confirmedMatches"></app-stat-card>
    </section>

    <mat-card class="quick-actions">
      <h3>Quick Actions</h3>
      <div class="action-row">
        <a mat-stroked-button routerLink="/admin/users">Manage users</a>
        <a mat-stroked-button routerLink="/admin/migrations">Migration status</a>
        <a mat-flat-button color="primary" routerLink="/admin/audit-logs">Audit logs</a>
      </div>
    </mat-card>
  `,
  styleUrl: './admin-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent {
  private readonly api = inject(AdminApiService);
  readonly dashboard = signal<AdminDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe((data) => this.dashboard.set(data));
  }
}

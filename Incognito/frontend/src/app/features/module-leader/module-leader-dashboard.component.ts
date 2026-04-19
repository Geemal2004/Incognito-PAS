import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ModuleLeaderApiService } from '../../core/services/module-leader-api.service';
import { ModuleLeaderDashboardDto } from '../../core/models/api.models';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { StatCardComponent } from '../../shared/ui/stat-card.component';

@Component({
  standalone: true,
  selector: 'app-module-leader-dashboard',
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, PageHeaderComponent, StatCardComponent],
  template: `
    <app-page-header title="Module Leader Dashboard" subtitle="Govern allocations, users, and taxonomy quality."></app-page-header>

    <section class="stats-grid" *ngIf="dashboard() as d">
      <app-stat-card label="Students" [value]="d.totalStudents"></app-stat-card>
      <app-stat-card label="Supervisors" [value]="d.totalSupervisors"></app-stat-card>
      <app-stat-card label="Proposals" [value]="d.totalProposals"></app-stat-card>
      <app-stat-card label="Matches" [value]="d.totalMatches"></app-stat-card>
    </section>

    <mat-card class="quick-actions">
      <h3>Quick Actions</h3>
      <div class="action-row">
        <a mat-stroked-button routerLink="/module-leader/matches">Match oversight</a>
        <a mat-stroked-button routerLink="/module-leader/research-areas">Research areas</a>
        <a mat-flat-button color="primary" routerLink="/module-leader/users">User directory</a>
      </div>
    </mat-card>
  `,
  styleUrl: './module-leader-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModuleLeaderDashboardComponent {
  private readonly api = inject(ModuleLeaderApiService);
  readonly dashboard = signal<ModuleLeaderDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe((data) => this.dashboard.set(data));
  }
}

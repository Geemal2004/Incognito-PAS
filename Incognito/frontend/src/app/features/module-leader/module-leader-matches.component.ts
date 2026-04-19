import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { ModuleLeaderMatchDto } from '../../core/models/api.models';
import { ModuleLeaderApiService } from '../../core/services/module-leader-api.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { FilterBarComponent } from '../../shared/ui/filter-bar.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { SearchBoxComponent } from '../../shared/ui/search-box.component';

@Component({
  selector: 'app-module-leader-matches',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    PageHeaderComponent,
    FilterBarComponent,
    SearchBoxComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './module-leader-matches.component.html',
  styleUrl: './module-leader-matches.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModuleLeaderMatchesComponent {
  private readonly api = inject(ModuleLeaderApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly search = signal('');
  readonly status = signal('');
  readonly matches = signal<ModuleLeaderMatchDto[]>([]);
  readonly supervisors = signal<{ id: string; fullName: string }[]>([]);
  readonly selectedSupervisorByMatch = signal<Record<number, string>>({});

  constructor() {
    this.loadSupervisors();
    this.loadMatches();
  }

  updateSearch(value: string): void {
    this.search.set(value);
    this.loadMatches();
  }

  updateStatus(value: string): void {
    this.status.set(value);
    this.loadMatches();
  }

  setSupervisor(matchId: number, supervisorId: string): void {
    this.selectedSupervisorByMatch.set({
      ...this.selectedSupervisorByMatch(),
      [matchId]: supervisorId
    });
  }

  reassign(matchId: number): void {
    const supervisorId = this.selectedSupervisorByMatch()[matchId];
    if (!supervisorId) {
      this.toast.error('Select a supervisor first.');
      return;
    }

    this.api.reassign(matchId, supervisorId).subscribe({
      next: () => {
        this.toast.success('Match reassigned successfully.');
        this.loadMatches();
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to reassign match.')
    });
  }

  private loadMatches(): void {
    this.loading.set(true);
    this.api.getMatches(this.status() || undefined, this.search() || undefined).subscribe({
      next: (matches) => {
        this.matches.set(matches);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load matches.');
      }
    });
  }

  private loadSupervisors(): void {
    this.api.getSupervisors().subscribe({
      next: (supervisors) => this.supervisors.set(supervisors.map((item) => ({ id: item.id, fullName: item.fullName }))),
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to load supervisors.')
    });
  }
}

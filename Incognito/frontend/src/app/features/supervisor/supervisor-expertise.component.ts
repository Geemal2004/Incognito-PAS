import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ResearchAreaDto } from '../../core/models/api.models';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { ExpertiseChipSelectorComponent } from '../../shared/ui/expertise-chip-selector.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

@Component({
  selector: 'app-supervisor-expertise',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    ExpertiseChipSelectorComponent
  ],
  templateUrl: './supervisor-expertise.component.html',
  styleUrl: './supervisor-expertise.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorExpertiseComponent {
  private readonly api = inject(SupervisorApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly allAreas = signal<ResearchAreaDto[]>([]);
  readonly selectedIds = signal<number[]>([]);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);

    this.api.getResearchAreas().subscribe({
      next: (areas) => {
        this.allAreas.set(areas);
        this.loadCurrentSelection();
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load research areas.');
      }
    });
  }

  updateSelection(next: number[]): void {
    this.selectedIds.set(next);
  }

  save(): void {
    this.saving.set(true);
    this.api.setExpertise(this.selectedIds()).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Expertise updated successfully.');
      },
      error: (error) => {
        this.saving.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to update expertise.');
      }
    });
  }

  private loadCurrentSelection(): void {
    this.api.getExpertise().subscribe({
      next: (selected) => {
        this.selectedIds.set(selected.map((item) => item.id));
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load current expertise.');
      }
    });
  }
}

import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { BlindProposalDto, PagedResult, ResearchAreaDto } from '../../core/models/api.models';
import { getProposalDocumentViewUrl } from '../../core/utils/document-url.util';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { FilterBarComponent } from '../../shared/ui/filter-bar.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { ProposalCardComponent } from '../../shared/ui/proposal-card.component';
import { SearchBoxComponent } from '../../shared/ui/search-box.component';

@Component({
  standalone: true,
  selector: 'app-supervisor-browse',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    PageHeaderComponent,
    FilterBarComponent,
    SearchBoxComponent,
    ProposalCardComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './supervisor-browse.component.html',
  styleUrl: './supervisor-browse.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorBrowseComponent {
  private readonly api = inject(SupervisorApiService);
  private readonly toast = inject(ToastService);

  readonly search = signal('');
  readonly researchArea = signal('');
  readonly sort = signal('newest');
  readonly loading = signal(true);
  readonly researchAreas = signal<ResearchAreaDto[]>([]);
  readonly results = signal<PagedResult<BlindProposalDto> | null>(null);

  constructor() {
    this.loadResearchAreas();
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.browse({
      search: this.search(),
      researchArea: this.researchArea(),
      sort: this.sort(),
      page: 1,
      pageSize: 20
    }).subscribe({
      next: (res) => {
        this.results.set(res);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load proposals.');
      }
    });
  }

  loadResearchAreas(): void {
    this.api.getResearchAreas().subscribe({
      next: (areas) => this.researchAreas.set(areas),
      error: () => {
        // Non-blocking for browse flow; proposals can still be searched and sorted.
        this.researchAreas.set([]);
      }
    });
  }

  updateSearch(value: string): void {
    this.search.set(value);
    this.load();
  }

  updateResearchArea(value: string): void {
    this.researchArea.set(value);
    this.load();
  }

  updateSort(value: string): void {
    this.sort.set(value);
    this.load();
  }

  expressInterest(proposalId: number): void {
    this.api.expressInterest(proposalId).subscribe({
      next: () => {
        this.toast.success('Interest recorded.');
        this.load();
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to express interest.')
    });
  }

  proposalDocumentViewUrl(url?: string | null): string | null {
    return getProposalDocumentViewUrl(url);
  }
}

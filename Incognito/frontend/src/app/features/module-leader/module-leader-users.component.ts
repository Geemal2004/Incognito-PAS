import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { UserSummaryDto } from '../../core/models/api.models';
import { ModuleLeaderApiService } from '../../core/services/module-leader-api.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { FilterBarComponent } from '../../shared/ui/filter-bar.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { SearchBoxComponent } from '../../shared/ui/search-box.component';

@Component({
  selector: 'app-module-leader-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    PageHeaderComponent,
    FilterBarComponent,
    SearchBoxComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './module-leader-users.component.html',
  styleUrl: './module-leader-users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModuleLeaderUsersComponent {
  private readonly api = inject(ModuleLeaderApiService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly users = signal<UserSummaryDto[]>([]);
  readonly search = signal('');
  readonly role = signal('');

  readonly filteredUsers = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.users();
    }

    return this.users().filter((user) => {
      return (
        user.fullName.toLowerCase().includes(term)
        || user.email.toLowerCase().includes(term)
        || user.role.toLowerCase().includes(term)
      );
    });
  });

  constructor() {
    this.load();
  }

  updateSearch(value: string): void {
    this.search.set(value);
  }

  updateRole(value: string): void {
    this.role.set(value);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getUsers(this.role() || undefined).subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load users.');
      }
    });
  }
}

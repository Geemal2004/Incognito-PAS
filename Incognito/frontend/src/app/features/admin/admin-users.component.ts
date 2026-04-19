import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserSummaryDto } from '../../core/models/api.models';
import { AdminApiService } from '../../core/services/admin-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { FilterBarComponent } from '../../shared/ui/filter-bar.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { SearchBoxComponent } from '../../shared/ui/search-box.component';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    FilterBarComponent,
    SearchBoxComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent
  ],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminUsersComponent {
  private readonly api = inject(AdminApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly users = signal<UserSummaryDto[]>([]);
  readonly search = signal('');
  readonly roleFilter = signal('');

  readonly createForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Student', Validators.required]
  });

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
    this.loadUsers();
  }

  updateSearch(value: string): void {
    this.search.set(value);
  }

  updateRoleFilter(value: string): void {
    this.roleFilter.set(value);
    this.loadUsers();
  }

  createUser(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.api.createUser(this.createForm.getRawValue()).subscribe({
      next: () => {
        this.toast.success('User account created.');
        this.createForm.reset({ fullName: '', email: '', password: '', role: 'Student' });
        this.loadUsers();
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to create user.')
    });
  }

  deactivateUser(userId: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Deactivate account',
        message: 'This account will lose access immediately. Continue?',
        confirmText: 'Deactivate'
      }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.api.deactivateUser(userId).subscribe({
        next: () => {
          this.toast.success('User deactivated successfully.');
          this.loadUsers();
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to deactivate user.')
      });
    });
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.api.getUsers(this.roleFilter() || undefined).subscribe({
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

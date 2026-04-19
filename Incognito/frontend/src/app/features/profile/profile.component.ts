import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { finalize, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

interface CloudinaryUploadResponse {
  secure_url: string;
}

const CLOUDINARY_CLOUD_NAME = 'dy3jmad0j';
const CLOUDINARY_UPLOAD_PRESET = 'profile_pics_preset';
const MAX_PROFILE_PICTURE_SIZE_BYTES = 5 * 1024 * 1024;

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, PageHeaderComponent, LoadingSkeletonComponent, EmptyStateComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly user = this.auth.currentUser;
  readonly avatarUrl = computed(() => this.user()?.profileImageUrl?.trim() || null);
  readonly initials = computed(() => {
    const fullName = this.user()?.fullName?.trim() ?? '';
    if (!fullName) {
      return 'U';
    }

    const parts = fullName.split(/\s+/).filter(Boolean);
    const first = parts[0]?.[0] ?? '';
    const second = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? '' : '';
    return `${first}${second}`.toUpperCase();
  });

  refresh(): void {
    this.loading.set(true);
    this.auth.getProfile().subscribe({
      next: (user) => {
        this.loading.set(false);
        if (user) {
          this.toast.success('Profile refreshed.');
        } else {
          this.toast.error('Unable to refresh profile.');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Unable to refresh profile.');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const selectedFile = input?.files?.[0];

    if (!selectedFile) {
      return;
    }

    if (!selectedFile.type.startsWith('image/')) {
      this.toast.error('Please choose an image file.');
      this.resetFileInput(input);
      return;
    }

    if (selectedFile.size > MAX_PROFILE_PICTURE_SIZE_BYTES) {
      this.toast.error('Profile picture must be 5MB or smaller.');
      this.resetFileInput(input);
      return;
    }

    const uploadForm = new FormData();
    uploadForm.append('file', selectedFile);
    uploadForm.append('upload_preset', CLOUDINARY_UPLOAD_PRESET);

    this.uploading.set(true);
    this.http
      .post<CloudinaryUploadResponse>(`https://api.cloudinary.com/v1_1/${CLOUDINARY_CLOUD_NAME}/image/upload`, uploadForm)
      .pipe(
        switchMap((uploadResponse) => {
          if (!uploadResponse?.secure_url) {
            return throwError(() => new Error('Cloudinary upload failed.'));
          }

          return this.auth.updateProfilePicture(uploadResponse.secure_url);
        }),
        finalize(() => {
          this.uploading.set(false);
          this.resetFileInput(input);
        })
      )
      .subscribe({
        next: (updatedUser) => {
          if (updatedUser) {
            this.toast.success('Profile picture updated.');
            return;
          }

          this.toast.error('Could not save profile picture. Please try again.');
        },
        error: () => {
          this.toast.error('Could not upload profile picture. Please try again.');
        }
      });
  }

  private resetFileInput(input: HTMLInputElement | null): void {
    if (input) {
      input.value = '';
    }
  }
}

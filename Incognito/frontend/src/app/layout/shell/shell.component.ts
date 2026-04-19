import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { map } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpoint = inject(BreakpointObserver);

  readonly user = this.auth.currentUser;
  readonly isCompact = toSignal(
    this.breakpoint.observe('(max-width: 1024px)').pipe(map((state) => state.matches)),
    { initialValue: false }
  );
  readonly workspaceTitle = computed(() => `${this.user()?.role ?? 'User'} Workspace`);

  readonly navLinks = computed(() => {
    const role = this.user()?.role;
    if (role === 'Student') {
      return [
        { label: 'Dashboard', link: '/student', icon: 'dashboard', exact: true },
        { label: 'My Profile', link: '/student/profile', icon: 'person' },
        { label: 'My Proposals', link: '/student/proposals', icon: 'description' },
        { label: 'Create Proposal', link: '/student/create', icon: 'add_circle' }
      ];
    }

    if (role === 'Supervisor') {
      return [
        { label: 'Dashboard', link: '/supervisor', icon: 'dashboard', exact: true },
        { label: 'My Profile', link: '/supervisor/profile', icon: 'person' },
        { label: 'Browse Blind Proposals', link: '/supervisor/browse', icon: 'travel_explore' },
        { label: 'My Interests', link: '/supervisor/interests', icon: 'favorite' },
        { label: 'Confirmed Matches', link: '/supervisor/confirmed', icon: 'verified' },
        { label: 'Expertise Profile', link: '/supervisor/expertise', icon: 'interests' }
      ];
    }

    if (role === 'ModuleLeader') {
      return [
        { label: 'Dashboard', link: '/module-leader', icon: 'dashboard', exact: true },
        { label: 'My Profile', link: '/module-leader/profile', icon: 'person' },
        { label: 'Match Oversight', link: '/module-leader/matches', icon: 'account_tree' },
        { label: 'Research Areas', link: '/module-leader/research-areas', icon: 'hub' },
        { label: 'User Directory', link: '/module-leader/users', icon: 'group' }
      ];
    }

    if (role === 'SysAdmin') {
      return [
        { label: 'Dashboard', link: '/admin', icon: 'dashboard', exact: true },
        { label: 'My Profile', link: '/admin/profile', icon: 'person' },
        { label: 'Users', link: '/admin/users', icon: 'manage_accounts' },
        { label: 'Migrations', link: '/admin/migrations', icon: 'build' },
        { label: 'Audit Logs', link: '/admin/audit-logs', icon: 'history' }
      ];
    }

    return [];
  });

  logout(): void {
    this.auth.logout();
  }

  closeOnMobile(drawer: MatSidenav): void {
    if (this.isCompact()) {
      void drawer.close();
    }
  }

  goHome(drawer?: MatSidenav): void {
    if (drawer && this.isCompact()) {
      void drawer.close();
    }

    void this.router.navigate(['/']);
  }

  userInitials(fullName: string): string {
    const name = fullName?.trim();
    if (!name) {
      return 'U';
    }

    const parts = name.split(/\s+/).filter(Boolean);
    const first = parts[0]?.[0] ?? '';
    const second = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? '' : '';
    return `${first}${second}`.toUpperCase();
  }
}

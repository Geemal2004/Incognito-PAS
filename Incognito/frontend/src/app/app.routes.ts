import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { rootRedirectGuard } from './core/guards/root-redirect.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
	{
		path: '',
		canActivate: [rootRedirectGuard],
		loadComponent: () => import('./features/public/landing.component').then((m) => m.LandingComponent)
	},
	{
		path: 'auth/login',
		loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent)
	},
	{
		path: 'auth/register',
		loadComponent: () => import('./features/auth/register.component').then((m) => m.RegisterComponent)
	},
	{
		path: 'auth/access-denied',
		loadComponent: () => import('./features/auth/access-denied.component').then((m) => m.AccessDeniedComponent)
	},
	{
		path: 'student',
		canActivate: [authGuard, roleGuard],
		data: { roles: ['Student'] },
		loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
		children: [
			{ path: '', loadComponent: () => import('./features/student/student-dashboard.component').then((m) => m.StudentDashboardComponent) },
			{ path: 'profile', loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent) },
			{ path: 'proposals', loadComponent: () => import('./features/student/student-proposals.component').then((m) => m.StudentProposalsComponent) },
			{ path: 'proposals/:id', loadComponent: () => import('./features/student/student-proposal-details.component').then((m) => m.StudentProposalDetailsComponent) },
			{ path: 'proposals/:id/edit', loadComponent: () => import('./features/student/proposal-form.component').then((m) => m.ProposalFormComponent) },
			{ path: 'create', loadComponent: () => import('./features/student/proposal-form.component').then((m) => m.ProposalFormComponent) }
		]
	},
	{
		path: 'supervisor',
		canActivate: [authGuard, roleGuard],
		data: { roles: ['Supervisor'] },
		loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
		children: [
			{ path: '', loadComponent: () => import('./features/supervisor/supervisor-dashboard.component').then((m) => m.SupervisorDashboardComponent) },
			{ path: 'profile', loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent) },
			{ path: 'browse', loadComponent: () => import('./features/supervisor/supervisor-browse.component').then((m) => m.SupervisorBrowseComponent) },
			{ path: 'interests', loadComponent: () => import('./features/supervisor/supervisor-interests.component').then((m) => m.SupervisorInterestsComponent) },
			{ path: 'confirmed', loadComponent: () => import('./features/supervisor/supervisor-confirmed.component').then((m) => m.SupervisorConfirmedComponent) },
			{ path: 'expertise', loadComponent: () => import('./features/supervisor/supervisor-expertise.component').then((m) => m.SupervisorExpertiseComponent) }
		]
	},
	{
		path: 'module-leader',
		canActivate: [authGuard, roleGuard],
		data: { roles: ['ModuleLeader'] },
		loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
		children: [
			{ path: '', loadComponent: () => import('./features/module-leader/module-leader-dashboard.component').then((m) => m.ModuleLeaderDashboardComponent) },
			{ path: 'profile', loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent) },
			{ path: 'matches', loadComponent: () => import('./features/module-leader/module-leader-matches.component').then((m) => m.ModuleLeaderMatchesComponent) },
			{ path: 'research-areas', loadComponent: () => import('./features/module-leader/module-leader-research-areas.component').then((m) => m.ModuleLeaderResearchAreasComponent) },
			{ path: 'users', loadComponent: () => import('./features/module-leader/module-leader-users.component').then((m) => m.ModuleLeaderUsersComponent) }
		]
	},
	{
		path: 'admin',
		canActivate: [authGuard, roleGuard],
		data: { roles: ['SysAdmin'] },
		loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
		children: [
			{ path: '', loadComponent: () => import('./features/admin/admin-dashboard.component').then((m) => m.AdminDashboardComponent) },
			{ path: 'profile', loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent) },
			{ path: 'users', loadComponent: () => import('./features/admin/admin-users.component').then((m) => m.AdminUsersComponent) },
			{ path: 'migrations', loadComponent: () => import('./features/admin/admin-migrations.component').then((m) => m.AdminMigrationsComponent) },
			{ path: 'audit-logs', loadComponent: () => import('./features/admin/admin-audit-logs.component').then((m) => m.AdminAuditLogsComponent) }
		]
	},
	{
		path: '**',
		loadComponent: () => import('./features/auth/not-found.component').then((m) => m.NotFoundComponent)
	}
];

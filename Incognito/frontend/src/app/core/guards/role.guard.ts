import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { AppRole } from '../models/api.models';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const roles = (route.data['roles'] as AppRole[] | undefined) ?? [];
  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  if (roles.length === 0 || auth.hasRole(roles)) {
    return true;
  }

  return router.createUrlTree(['/auth/access-denied']);
};

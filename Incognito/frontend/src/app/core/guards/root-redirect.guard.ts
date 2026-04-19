import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const rootRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return true;
  }

  const role = auth.currentUser()?.role;
  const target = role === 'Student' ? '/student' :
    role === 'Supervisor' ? '/supervisor' :
    role === 'ModuleLeader' ? '/module-leader' :
    role === 'SysAdmin' ? '/admin' : null;

  if (!target) {
    return true;
  }

  return router.createUrlTree([target]);
};
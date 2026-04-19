import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { API_BASE_URL } from '../services/api.config';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getToken();
  const isBackendApiRequest = req.url.startsWith(API_BASE_URL) || req.url.startsWith('/api/');
  const request = token && isBackendApiRequest
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.logout(false);
        void router.navigate(['/auth/login']);
      }

      if (error.status === 403) {
        void router.navigate(['/auth/access-denied']);
      }

      return throwError(() => error);
    })
  );
};

import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { roleGuard } from './role.guard';

describe('roleGuard', () => {
  const authServiceMock = {
    isAuthenticated: jasmine.createSpy('isAuthenticated'),
    hasRole: jasmine.createSpy('hasRole')
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceMock }]
    });

    authServiceMock.isAuthenticated.calls.reset();
    authServiceMock.hasRole.calls.reset();
  });

  it('redirects unauthenticated users to login', () => {
    authServiceMock.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() =>
      roleGuard({ data: { roles: ['Student'] } } as never, {} as never)
    );

    const router = TestBed.inject(Router);
    expect(result).toEqual(router.createUrlTree(['/auth/login']));
  });
});

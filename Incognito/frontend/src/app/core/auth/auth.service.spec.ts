import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthService } from './auth.service';
import { authInterceptor } from '../interceptors/auth.interceptor';
import { API_BASE_URL, API_ENDPOINTS } from '../services/api.config';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    localStorage.clear();
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('stores user and token on successful login', () => {
    service.login({ email: 'student@test.com', password: 'Password@123', rememberMe: true }).subscribe((res) => {
      expect(res.success).toBeTrue();
    });

    const req = httpMock.expectOne(`${API_BASE_URL}${API_ENDPOINTS.auth.login}`);
    req.flush({
      success: true,
      token: 'sample-token',
      user: { id: '1', email: 'student@test.com', fullName: 'Demo Student', role: 'Student' }
    });

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.getToken()).toBe('sample-token');
  });
});

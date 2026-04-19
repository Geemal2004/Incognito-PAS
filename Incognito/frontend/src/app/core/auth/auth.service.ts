import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from '../services/api.config';
import { AppRole, AuthResponse, AuthUser } from '../models/api.models';

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: 'Student' | 'Supervisor';
}

const TOKEN_KEY = 'incognito_token';
const USER_KEY = 'incognito_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly userState = signal<AuthUser | null>(this.loadUser());
  readonly currentUser = computed(() => this.userState());
  readonly isAuthenticated = computed(() => !!this.userState());

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}${API_ENDPOINTS.auth.login}`, request).pipe(
      tap((response) => this.persistAuth(response)),
      catchError((error) => of({ success: false, message: error?.error?.message ?? 'Login failed.' }))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}${API_ENDPOINTS.auth.register}`, request).pipe(
      tap((response) => this.persistAuth(response)),
      catchError((error) => of({ success: false, message: error?.error?.message ?? 'Registration failed.' }))
    );
  }

  getProfile(): Observable<AuthUser | null> {
    return this.http.get<AuthUser>(`${API_BASE_URL}${API_ENDPOINTS.auth.me}`).pipe(
      tap((user) => {
        this.persistUser(user);
      }),
      map((user) => user),
      catchError(() => of(null))
    );
  }

  updateProfilePicture(profileImageUrl: string): Observable<AuthUser | null> {
    return this.http.put<AuthUser>(`${API_BASE_URL}${API_ENDPOINTS.auth.profilePicture}`, { profileImageUrl }).pipe(
      tap((user) => this.persistUser(user)),
      map((user) => user),
      catchError(() => of(null))
    );
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(roles: AppRole[]): boolean {
    const role = this.userState()?.role;
    return !!role && roles.includes(role);
  }

  logout(redirect = true): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userState.set(null);
    if (redirect) {
      void this.router.navigate(['/auth/login']);
    }
  }

  navigateByRole(role?: string): void {
    const targetRole = role ?? this.currentUser()?.role;
    const path = targetRole === 'Student' ? '/student' :
      targetRole === 'Supervisor' ? '/supervisor' :
      targetRole === 'ModuleLeader' ? '/module-leader' :
      targetRole === 'SysAdmin' ? '/admin' : '/';

    void this.router.navigate([path]);
  }

  private persistAuth(response: AuthResponse): void {
    if (!response.success || !response.token || !response.user) {
      return;
    }

    localStorage.setItem(TOKEN_KEY, response.token);
    this.persistUser(response.user);
  }

  private persistUser(user: AuthUser): void {
    this.userState.set(user);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  private loadUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}

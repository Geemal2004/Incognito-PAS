import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from './api.config';
import { AdminDashboardDto, AuditLogDto, PagedResult, UserSummaryDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<AdminDashboardDto> {
    return this.http.get<AdminDashboardDto>(`${API_BASE_URL}${API_ENDPOINTS.admin.dashboard}`);
  }

  getUsers(role?: string): Observable<UserSummaryDto[]> {
    const params = role ? new HttpParams().set('role', role) : undefined;
    return this.http.get<UserSummaryDto[]>(`${API_BASE_URL}${API_ENDPOINTS.admin.users}`, { params });
  }

  createUser(payload: { fullName: string; email: string; password: string; role: string }): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.admin.users}`, payload);
  }

  deactivateUser(id: string): Observable<unknown> {
    return this.http.patch(`${API_BASE_URL}${API_ENDPOINTS.admin.users}/${id}/deactivate`, {});
  }

  getMigrations(): Observable<{ migrations: string[] }> {
    return this.http.get<{ migrations: string[] }>(`${API_BASE_URL}${API_ENDPOINTS.admin.migrations}`);
  }

  getAuditLogs(query?: { page?: number; pageSize?: number; entityType?: string; action?: string }): Observable<PagedResult<AuditLogDto>> {
    let params = new HttpParams();

    Object.entries(query ?? { page: 1, pageSize: 50 }).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return this.http.get<PagedResult<AuditLogDto>>(`${API_BASE_URL}${API_ENDPOINTS.admin.auditLogs}`, { params });
  }
}

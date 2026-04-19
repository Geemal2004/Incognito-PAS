import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from './api.config';
import { ModuleLeaderDashboardDto, ModuleLeaderMatchDto, ResearchAreaDto, UserSummaryDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ModuleLeaderApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<ModuleLeaderDashboardDto> {
    return this.http.get<ModuleLeaderDashboardDto>(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.dashboard}`);
  }

  getMatches(status?: string, search?: string): Observable<ModuleLeaderMatchDto[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<ModuleLeaderMatchDto[]>(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.matches}`, { params });
  }

  getUsers(role?: string): Observable<UserSummaryDto[]> {
    const params = role ? new HttpParams().set('role', role) : undefined;
    return this.http.get<UserSummaryDto[]>(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.users}`, { params });
  }

  getSupervisors(): Observable<UserSummaryDto[]> {
    return this.getUsers('Supervisor');
  }

  getResearchAreas(): Observable<ResearchAreaDto[]> {
    return this.http.get<ResearchAreaDto[]>(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.researchAreas}`);
  }

  createResearchArea(payload: Pick<ResearchAreaDto, 'name' | 'description'>): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.researchAreas}`, payload);
  }

  updateResearchArea(id: number, payload: { name: string; description?: string; isActive: boolean }): Observable<unknown> {
    return this.http.put(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.researchAreas}/${id}`, payload);
  }

  deleteResearchArea(id: number): Observable<unknown> {
    return this.http.delete(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.researchAreas}/${id}`);
  }

  reassign(matchId: number, newSupervisorId: string): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.moduleLeader.reassignments}`, { matchId, newSupervisorId });
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from './api.config';
import { BlindProposalDto, MatchSummaryDto, PagedResult, ResearchAreaDto, SupervisorDashboardDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class SupervisorApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<SupervisorDashboardDto> {
    return this.http.get<SupervisorDashboardDto>(`${API_BASE_URL}${API_ENDPOINTS.supervisor.dashboard}`);
  }

  browse(query: { search?: string; researchArea?: string; sort?: string; page?: number; pageSize?: number }): Observable<PagedResult<BlindProposalDto>> {
    let params = new HttpParams();
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });

    if (!params.has('sort')) {
      params = params.set('sort', 'newest');
    }

    return this.http.get<PagedResult<BlindProposalDto>>(`${API_BASE_URL}${API_ENDPOINTS.supervisor.browse}`, { params });
  }

  expressInterest(proposalId: number): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.supervisor.browse.replace('/browse', `/interests/${proposalId}`)}`, {});
  }

  getMatches(): Observable<{ interests: MatchSummaryDto[]; confirmed: MatchSummaryDto[] }> {
    return this.http.get<{ interests: MatchSummaryDto[]; confirmed: MatchSummaryDto[] }>(`${API_BASE_URL}${API_ENDPOINTS.supervisor.matches}`);
  }

  confirmMatch(matchId: number): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.supervisor.matches}/${matchId}/confirm`, { confirmed: true });
  }

  getExpertise(): Observable<ResearchAreaDto[]> {
    return this.http.get<ResearchAreaDto[]>(`${API_BASE_URL}${API_ENDPOINTS.supervisor.expertise}`);
  }

  setExpertise(areaIds: number[]): Observable<unknown> {
    return this.http.put(`${API_BASE_URL}${API_ENDPOINTS.supervisor.expertise}`, { areaIds });
  }

  getResearchAreas(): Observable<ResearchAreaDto[]> {
    return this.http.get<ResearchAreaDto[]>(`${API_BASE_URL}${API_ENDPOINTS.supervisor.researchAreas}`);
  }
}

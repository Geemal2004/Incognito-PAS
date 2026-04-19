export type AppRole = 'Student' | 'Supervisor' | 'ModuleLeader' | 'SysAdmin';
export type ProposalStatus = 'Pending' | 'UnderReview' | 'Matched' | 'Withdrawn' | string;

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  role: AppRole;
  profileImageUrl?: string | null;
}

export interface AuthResponse {
  success: boolean;
  token?: string;
  message?: string;
  user?: AuthUser;
}

export interface ApiErrorDto {
  message: string;
  code?: string;
}

export interface PagedResult<T> {
  page: number;
  pageSize: number;
  total: number;
  items: T[];
}

export interface RevealDetailsDto {
  matchId: number;
  supervisorName: string;
  supervisorEmail: string;
  confirmedAt?: string;
}

export interface ProposalListItemDto {
  id: number;
  title: string;
  abstract: string;
  technicalStack?: string;
  proposalDocumentUrl?: string;
  researchAreaName: string;
  status: ProposalStatus;
  createdAt: string;
  isGroupProject?: boolean;
  projectGroupId?: number;
  projectGroupName?: string;
  groupMembers?: string[];
  canManage?: boolean;
  matchedSupervisor?: RevealDetailsDto;
}

export interface ProposalDetailsDto extends ProposalListItemDto {}

export interface BlindProposalDto {
  id: number;
  title: string;
  abstract: string;
  technicalStack?: string;
  proposalDocumentUrl?: string;
  researchAreaName: string;
  status: ProposalStatus;
  createdAt: string;
}

export interface ResearchAreaDto {
  id: number;
  name: string;
  description?: string;
}

export interface ProjectGroupMemberDto {
  studentId: string;
  fullName: string;
  email: string;
  isLeader: boolean;
}

export interface ProjectGroupDto {
  id: number;
  name: string;
  leaderId: string;
  leaderName: string;
  createdAt: string;
  members: ProjectGroupMemberDto[];
}

export interface StudentPeerDto {
  id: string;
  fullName: string;
  email: string;
}

export interface MatchSummaryDto {
  matchId: number;
  proposalId: number;
  title?: string;
  abstract?: string;
  technicalStack?: string;
  researchAreaName?: string;
  studentName?: string;
  studentEmail?: string;
  isGroupProject?: boolean;
  projectGroupName?: string;
  groupMembers?: string[];
  projectTitle?: string;
  confirmedAt?: string;
}

export interface ModuleLeaderMatchDto {
  matchId: number;
  studentName: string;
  studentEmail: string;
  supervisorName: string;
  supervisorEmail: string;
  projectTitle: string;
  researchArea: string;
  status: ProposalStatus;
  isConfirmed: boolean;
  confirmedAt?: string;
}

export interface StudentDashboardDto {
  totalProposals: number;
  pendingCount: number;
  underReviewCount: number;
  matchedCount: number;
  withdrawnCount: number;
  recentProposals: ProposalListItemDto[];
}

export interface SupervisorDashboardDto {
  totalInterests: number;
  confirmedMatches: number;
  expertiseAreas: number;
  expertise: ResearchAreaDto[];
  interests: MatchSummaryDto[];
  confirmed: MatchSummaryDto[];
}

export interface ModuleLeaderDashboardDto {
  totalStudents: number;
  totalSupervisors: number;
  totalProposals: number;
  totalMatches: number;
  recentMatches: {
    matchId: number;
    studentName: string;
    studentEmail: string;
    supervisorName: string;
    supervisorEmail: string;
    projectTitle: string;
    researchArea: string;
    status: string;
    isConfirmed: boolean;
    confirmedAt?: string;
  }[];
}

export interface AdminDashboardDto {
  totalUsers: number;
  totalProposals: number;
  pendingProposals: number;
  totalMatches: number;
  confirmedMatches: number;
}

export interface UserSummaryDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
}

export interface AuditLogDto {
  id: number;
  action: string;
  entityType: string;
  entityId: string;
  actorUserId?: string;
  oldValues?: string;
  newValues?: string;
  timestamp: string;
  ipAddress?: string;
}

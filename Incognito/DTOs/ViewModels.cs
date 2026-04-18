namespace IncognitoPAS.DTOs;

// STUDENT VIEW MODELS
public class StudentProposalDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string? TechnicalStack { get; set; }
    public string? ProposalDocumentUrl { get; set; }
    public string ResearchAreaName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsGroupProject { get; set; }
    public int? ProjectGroupId { get; set; }
    public string? ProjectGroupName { get; set; }
    public List<string> GroupMembers { get; set; } = new();
    public bool CanManage { get; set; }
    public RevealedMatchDto? MatchedSupervisor { get; set; }
}

public class ProjectGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LeaderId { get; set; } = string.Empty;
    public string LeaderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ProjectGroupMemberDto> Members { get; set; } = new();
}

public class ProjectGroupMemberDto
{
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsLeader { get; set; }
}

public class StudentPeerDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class RevealedMatchDto
{
    public int MatchId { get; set; }
    public string SupervisorName { get; set; } = string.Empty;
    public string SupervisorEmail { get; set; } = string.Empty;
    public DateTime? ConfirmedAt { get; set; }
}

// SUPERVISOR VIEW MODELS
public class SupervisorDashboardDto
{
    public int TotalInterests { get; set; }
    public int ConfirmedMatches { get; set; }
    public int ExpertiseAreas { get; set; }
    public List<SupervisorInterestDto> Interests { get; set; } = new();
    public List<SupervisorConfirmedMatchDto> ConfirmedMatchesList { get; set; } = new();
}

public class SupervisorInterestDto
{
    public int MatchId { get; set; }
    public int ProposalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string? TechnicalStack { get; set; }
    public string ResearchAreaName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SupervisorConfirmedMatchDto
{
    public int MatchId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public DateTime? ConfirmedAt { get; set; }
}

// MODULE LEADER VIEW MODELS
public class ModuleLeaderDashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalProposals { get; set; }
    public int TotalMatches { get; set; }
    public List<MatchOverviewDto> Matches { get; set; } = new();
}

public class MatchOverviewDto
{
    public int MatchId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string SupervisorEmail { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

// ADMIN VIEW MODELS
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalProposals { get; set; }
    public int PendingProposals { get; set; }
    public int TotalMatches { get; set; }
    public int ConfirmedMatches { get; set; }
}

// Common
public class UserListDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

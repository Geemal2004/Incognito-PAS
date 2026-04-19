using IncognitoPAS.DTOs;

namespace IncognitoPAS.DTOs.Api;

public class ModuleLeaderDashboardResponseDto
{
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalProposals { get; set; }
    public int TotalMatches { get; set; }
    public List<MatchOverviewDto> RecentMatches { get; set; } = new();
}

public class ReassignRequestDto
{
    public int MatchId { get; set; }
    public string NewSupervisorId { get; set; } = string.Empty;
}

public class ResearchAreaWriteDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

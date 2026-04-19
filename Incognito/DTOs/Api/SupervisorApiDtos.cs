using IncognitoPAS.Models;

namespace IncognitoPAS.DTOs.Api;

public class SupervisorDashboardDto
{
    public int TotalInterests { get; set; }
    public int ConfirmedMatches { get; set; }
    public int ExpertiseAreas { get; set; }
    public List<ResearchAreaDto> Expertise { get; set; } = new();
    public List<SupervisorInterestDto> Interests { get; set; } = new();
    public List<SupervisorConfirmedMatchDto> Confirmed { get; set; } = new();
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
    public int ProposalId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public bool IsGroupProject { get; set; }
    public string? ProjectGroupName { get; set; }
    public List<string> GroupMembers { get; set; } = new();
    public string ProjectTitle { get; set; } = string.Empty;
    public DateTime? ConfirmedAt { get; set; }
}

public class SupervisorMatchListDto
{
    public List<SupervisorInterestDto> Interests { get; set; } = new();
    public List<SupervisorConfirmedMatchDto> Confirmed { get; set; } = new();
}

public class ExpertiseUpdateDto
{
    public List<int> AreaIds { get; set; } = new();
}

public class ResearchAreaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BrowseQueryDto
{
    public string? Search { get; set; }
    public string? ResearchArea { get; set; }
    public string Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ConfirmMatchRequestDto
{
    public bool Confirmed { get; set; }
}

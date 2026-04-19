using System.ComponentModel.DataAnnotations;
using IncognitoPAS.DTOs;

namespace IncognitoPAS.DTOs.Api;

public class StudentDashboardDto
{
    public int TotalProposals { get; set; }
    public int PendingCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int MatchedCount { get; set; }
    public int WithdrawnCount { get; set; }
    public List<StudentProposalDto> RecentProposals { get; set; } = new();
}

public class ProposalUpsertDto
{
    [Required]
    [StringLength(150, MinimumLength = 10)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 100)]
    public string Abstract { get; set; } = string.Empty;

    [StringLength(200)]
    public string? TechnicalStack { get; set; }

    [StringLength(500)]
    public string? ProposalDocumentUrl { get; set; }

    [Required]
    public int ResearchAreaId { get; set; }

    public int? ProjectGroupId { get; set; }
}

public class CreateProjectGroupRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<string> MemberStudentIds { get; set; } = new();
}

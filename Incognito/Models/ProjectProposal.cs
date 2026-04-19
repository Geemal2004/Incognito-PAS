using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncognitoPAS.Models;

public enum ProposalStatus { Pending, UnderReview, Matched, Withdrawn }

public class ProjectProposal
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    [MinLength(10)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [MinLength(100)]
    public string Abstract { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TechnicalStack { get; set; }

    [MaxLength(500)]
    public string? ProposalDocumentUrl { get; set; }

    public int ResearchAreaId { get; set; }

    [ForeignKey(nameof(ResearchAreaId))]
    public ResearchArea? ResearchArea { get; set; }

    public int? ProjectGroupId { get; set; }

    [ForeignKey(nameof(ProjectGroupId))]
    public ProjectGroup? ProjectGroup { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    [ForeignKey(nameof(StudentId))]
    public ApplicationUser? Student { get; set; }

    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    public bool IsAnonymous { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<SupervisorMatch> SupervisorMatches { get; set; } = new List<SupervisorMatch>();
}

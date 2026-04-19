using System.ComponentModel.DataAnnotations;

namespace IncognitoPAS.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    // Actions: InterestExpressed, MatchConfirmed, IdentityRevealed, ProposalWithdrawn, ProposalReassigned

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty; // "ProjectProposal", "SupervisorMatch"

    [Required]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? ActorUserId { get; set; }

    public string? OldValues { get; set; } // JSON snapshot

    public string? NewValues { get; set; } // JSON snapshot

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }
}

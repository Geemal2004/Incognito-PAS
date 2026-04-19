using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncognitoPAS.Models;

public class SupervisorMatch
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    [ForeignKey(nameof(ProposalId))]
    public ProjectProposal? Proposal { get; set; }

    [Required]
    public string SupervisorId { get; set; } = string.Empty;

    [ForeignKey(nameof(SupervisorId))]
    public ApplicationUser? Supervisor { get; set; }

    public bool IsConfirmed { get; set; } = false;

    public DateTime? ConfirmedAt { get; set; }

    // Only true after supervisor confirms - this triggers identity reveal
    public bool IsRevealed { get; set; } = false;

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncognitoPAS.Models;

public class ProjectGroup
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string LeaderId { get; set; } = string.Empty;

    [ForeignKey(nameof(LeaderId))]
    public ApplicationUser? Leader { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectGroupMember> Members { get; set; } = new List<ProjectGroupMember>();
    public ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
}

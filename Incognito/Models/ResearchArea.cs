using System.ComponentModel.DataAnnotations;

namespace IncognitoPAS.Models;

public class ResearchArea
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
    public ICollection<SupervisorExpertise> SupervisorExpertises { get; set; } = new List<SupervisorExpertise>();
}

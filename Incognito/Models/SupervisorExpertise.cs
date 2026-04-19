using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncognitoPAS.Models;

public class SupervisorExpertise
{
    public int Id { get; set; }

    [Required]
    public string SupervisorId { get; set; } = string.Empty;

    [ForeignKey(nameof(SupervisorId))]
    public ApplicationUser? Supervisor { get; set; }

    public int ResearchAreaId { get; set; }

    [ForeignKey(nameof(ResearchAreaId))]
    public ResearchArea? ResearchArea { get; set; }
}

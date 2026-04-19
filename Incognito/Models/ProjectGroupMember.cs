using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncognitoPAS.Models;

public class ProjectGroupMember
{
    public int Id { get; set; }

    public int ProjectGroupId { get; set; }

    [ForeignKey(nameof(ProjectGroupId))]
    public ProjectGroup? ProjectGroup { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    [ForeignKey(nameof(StudentId))]
    public ApplicationUser? Student { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

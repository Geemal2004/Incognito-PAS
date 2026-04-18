using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IncognitoPAS.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    // NOTE: Role is NO LONGER stored here. Use ASP.NET Identity roles via UserManager.GetRolesAsync()
    // This ensures role consistency with the Identity system

    public ICollection<SupervisorExpertise> ExpertiseTags { get; set; } = new List<SupervisorExpertise>();
    public ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
    public ICollection<ProjectGroupMember> ProjectGroupMemberships { get; set; } = new List<ProjectGroupMember>();
    public ICollection<ProjectGroup> LedProjectGroups { get; set; } = new List<ProjectGroup>();
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public ProposalService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal, string studentId)
    {
        ValidateProposalContent(proposal);

        var user = await _userManager.FindByIdAsync(studentId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Student"))
        {
            throw new InvalidOperationException("Only students can create proposals.");
        }

        var area = await _context.ResearchAreas.FindAsync(proposal.ResearchAreaId);
        if (area == null)
        {
            throw new InvalidOperationException("Invalid research area.");
        }

        if (proposal.ProjectGroupId.HasValue)
        {
            var group = await _context.ProjectGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == proposal.ProjectGroupId.Value);

            if (group == null)
            {
                throw new InvalidOperationException("Selected group was not found.");
            }

            if (!group.Members.Any(m => m.StudentId == studentId))
            {
                throw new UnauthorizedAccessException("You can only submit proposals for groups you belong to.");
            }

            if (group.LeaderId != studentId)
            {
                throw new InvalidOperationException("Only the group leader can submit a group proposal.");
            }

            var hasActiveGroupProposal = await _context.Proposals
                .AnyAsync(p => p.ProjectGroupId == group.Id && p.Status != ProposalStatus.Withdrawn);

            if (hasActiveGroupProposal)
            {
                throw new InvalidOperationException("This group already has an active proposal.");
            }

            proposal.StudentId = group.LeaderId;
            proposal.ProjectGroupId = group.Id;
        }
        else
        {
            proposal.StudentId = studentId;
        }

        proposal.Status = ProposalStatus.Pending;
        proposal.IsAnonymous = true;
        proposal.CreatedAt = DateTime.UtcNow;
        proposal.UpdatedAt = DateTime.UtcNow;

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        return proposal;
    }

    public async Task<IEnumerable<StudentProposalDto>> GetProposalsByStudentAsync(string studentId)
    {
        var proposals = await _context.Proposals
            .Include(p => p.ResearchArea)
            .Include(p => p.SupervisorMatches)
            .Include(p => p.ProjectGroup)
                .ThenInclude(g => g!.Members)
                    .ThenInclude(m => m.Student)
            .Where(p => p.StudentId == studentId ||
                        (p.ProjectGroupId.HasValue && p.ProjectGroup != null && p.ProjectGroup.Members.Any(m => m.StudentId == studentId)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var result = new List<StudentProposalDto>();
        foreach (var p in proposals)
        {
            var dto = new StudentProposalDto
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechnicalStack = p.TechnicalStack,
                ProposalDocumentUrl = p.ProposalDocumentUrl,
                ResearchAreaName = p.ResearchArea?.Name ?? "",
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                IsGroupProject = p.ProjectGroupId.HasValue,
                ProjectGroupId = p.ProjectGroupId,
                ProjectGroupName = p.ProjectGroup?.Name,
                GroupMembers = p.ProjectGroup?.Members
                    .OrderBy(m => m.Student?.FullName ?? string.Empty)
                    .Select(m => m.Student?.FullName ?? "Unknown Student")
                    .ToList() ?? new List<string>(),
                CanManage = p.StudentId == studentId
            };

            // If matched, get revealed supervisor info
            if (p.Status == ProposalStatus.Matched)
            {
                var match = await _context.SupervisorMatches
                    .Include(m => m.Supervisor)
                    .FirstOrDefaultAsync(m => m.ProposalId == p.Id && m.IsRevealed);

                if (match != null)
                {
                    dto.MatchedSupervisor = new RevealedMatchDto
                    {
                        MatchId = match.Id,
                        SupervisorName = match.Supervisor?.FullName ?? "",
                        SupervisorEmail = match.Supervisor?.Email ?? "",
                        ConfirmedAt = match.ConfirmedAt
                    };
                }
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<ProjectProposal?> UpdateProposalAsync(ProjectProposal proposal, string studentId)
    {
        var existing = await _context.Proposals.FindAsync(proposal.Id);
        if (existing == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (existing.StudentId != studentId)
        {
            if (existing.ProjectGroupId.HasValue)
            {
                throw new UnauthorizedAccessException("Only the group leader can edit group proposals.");
            }

            throw new UnauthorizedAccessException("You can only edit your own proposals.");
        }

        if (existing.Status != ProposalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending proposals can be edited.");
        }

        ValidateProposalContent(proposal);

        if (proposal.ProjectGroupId.HasValue && proposal.ProjectGroupId != existing.ProjectGroupId)
        {
            throw new InvalidOperationException("Group selection cannot be changed after proposal submission.");
        }

        existing.Title = proposal.Title;
        existing.Abstract = proposal.Abstract;
        existing.TechnicalStack = proposal.TechnicalStack;
        existing.ProposalDocumentUrl = proposal.ProposalDocumentUrl;
        existing.ResearchAreaId = proposal.ResearchAreaId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task WithdrawProposalAsync(int proposalId, string studentId, string? ipAddress = null)
    {
        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (proposal.StudentId != studentId)
        {
            if (proposal.ProjectGroupId.HasValue)
            {
                throw new UnauthorizedAccessException("Only the group leader can withdraw group proposals.");
            }

            throw new UnauthorizedAccessException("You can only withdraw your own proposals.");
        }

        if (proposal.Status == ProposalStatus.Matched)
        {
            throw new InvalidOperationException("Cannot withdraw a matched proposal.");
        }

        var oldStatus = proposal.Status;
        proposal.Status = ProposalStatus.Withdrawn;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "ProposalWithdrawn",
            nameof(ProjectProposal),
            proposalId.ToString(),
            studentId,
            new { OldStatus = oldStatus.ToString() },
            new { NewStatus = ProposalStatus.Withdrawn.ToString() },
            ipAddress);
    }

    public async Task<IEnumerable<BlindProposalDto>> GetAnonymousProposalsForSupervisorAsync(string supervisorId)
    {
        // Get supervisor's expertise areas - NO loading of Student entity
        var supervisorAreas = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchAreaId)
            .ToListAsync();

        // Project directly to BlindProposalDto - Student identity never loaded
        var proposals = await _context.Proposals
            .Where(p => (p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview)
                        && supervisorAreas.Contains(p.ResearchAreaId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BlindProposalDto
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechnicalStack = p.TechnicalStack,
                ProposalDocumentUrl = p.ProposalDocumentUrl,
                ResearchAreaName = p.ResearchArea != null ? p.ResearchArea.Name : "Unknown",
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
                // ⛔ SECURITY: StudentId, StudentName, StudentEmail - NEVER projected
            })
            .ToListAsync();

        return proposals;
    }

    public async Task<ProjectGroupDto> CreateProjectGroupAsync(string leaderId, string name, IEnumerable<string> memberStudentIds)
    {
        var normalizedName = (name ?? string.Empty).Trim();
        if (normalizedName.Length is < 3 or > 100)
        {
            throw new ValidationException("Group name must be between 3 and 100 characters.");
        }

        var leader = await _userManager.FindByIdAsync(leaderId);
        if (leader == null)
        {
            throw new InvalidOperationException("Leader student account was not found.");
        }

        var leaderRoles = await _userManager.GetRolesAsync(leader);
        if (!leaderRoles.Contains("Student"))
        {
            throw new InvalidOperationException("Only students can create project groups.");
        }

        var requestedMemberIds = memberStudentIds
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Where(id => id != leaderId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (requestedMemberIds.Count == 0)
        {
            throw new ValidationException("A group must include at least one additional student.");
        }

        var requestedMembers = await _context.Users
            .Where(u => requestedMemberIds.Contains(u.Id))
            .ToListAsync();

        if (requestedMembers.Count != requestedMemberIds.Count)
        {
            throw new InvalidOperationException("One or more selected students were not found.");
        }

        foreach (var member in requestedMembers)
        {
            if (!await _userManager.IsInRoleAsync(member, "Student"))
            {
                throw new InvalidOperationException("Groups can only include student accounts.");
            }
        }

        var group = new ProjectGroup
        {
            Name = normalizedName,
            LeaderId = leaderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        group.Members.Add(new ProjectGroupMember
        {
            StudentId = leaderId,
            JoinedAt = DateTime.UtcNow
        });

        foreach (var memberId in requestedMemberIds)
        {
            group.Members.Add(new ProjectGroupMember
            {
                StudentId = memberId,
                JoinedAt = DateTime.UtcNow
            });
        }

        _context.ProjectGroups.Add(group);
        await _context.SaveChangesAsync();

        var persistedGroup = await _context.ProjectGroups
            .Include(g => g.Leader)
            .Include(g => g.Members)
                .ThenInclude(m => m.Student)
            .SingleAsync(g => g.Id == group.Id);

        return MapGroupDto(persistedGroup);
    }

    public async Task<IEnumerable<ProjectGroupDto>> GetProjectGroupsForStudentAsync(string studentId)
    {
        var groups = await _context.ProjectGroups
            .Include(g => g.Leader)
            .Include(g => g.Members)
                .ThenInclude(m => m.Student)
            .Where(g => g.Members.Any(m => m.StudentId == studentId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return groups.Select(MapGroupDto).ToList();
    }

    public async Task<IEnumerable<StudentPeerDto>> GetStudentPeersAsync(string studentId)
    {
        var peers = await (from user in _context.Users
                           join userRole in _context.UserRoles on user.Id equals userRole.UserId
                           join role in _context.Roles on userRole.RoleId equals role.Id
                           where role.Name == "Student" && user.Id != studentId
                           orderby user.FullName
                           select new StudentPeerDto
                           {
                               Id = user.Id,
                               FullName = user.FullName,
                               Email = user.Email ?? string.Empty
                           })
            .Distinct()
            .ToListAsync();

        return peers;
    }

    private static void ValidateProposalContent(ProjectProposal proposal)
    {
        if (string.IsNullOrWhiteSpace(proposal.Title) || proposal.Title.Length < 10 || proposal.Title.Length > 150)
        {
            throw new ValidationException("Title must be between 10 and 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(proposal.Abstract) || proposal.Abstract.Length < 100 || proposal.Abstract.Length > 1000)
        {
            throw new ValidationException("Abstract must be between 100 and 1000 characters.");
        }

        proposal.ProposalDocumentUrl = NormalizeProposalDocumentUrl(proposal.ProposalDocumentUrl);
    }

    private static string? NormalizeProposalDocumentUrl(string? proposalDocumentUrl)
    {
        if (string.IsNullOrWhiteSpace(proposalDocumentUrl))
        {
            return null;
        }

        var normalizedUrl = proposalDocumentUrl.Trim();

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Host, "res.cloudinary.com", StringComparison.OrdinalIgnoreCase)
            || !uri.AbsolutePath.Contains("/dy3jmad0j/", StringComparison.OrdinalIgnoreCase)
            || !uri.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Proposal document URL must be a valid Cloudinary PDF URL.");
        }

        return normalizedUrl;
    }

    private static ProjectGroupDto MapGroupDto(ProjectGroup group)
    {
        return new ProjectGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            LeaderId = group.LeaderId,
            LeaderName = group.Leader?.FullName ?? "Unknown Student",
            CreatedAt = group.CreatedAt,
            Members = group.Members
                .OrderBy(m => m.Student?.FullName)
                .Select(member => new ProjectGroupMemberDto
                {
                    StudentId = member.StudentId,
                    FullName = member.Student?.FullName ?? "Unknown Student",
                    Email = member.Student?.Email ?? string.Empty,
                    IsLeader = member.StudentId == group.LeaderId
                })
                .ToList()
        };
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

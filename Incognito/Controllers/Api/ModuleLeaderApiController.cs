using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs;
using IncognitoPAS.DTOs.Api;
using IncognitoPAS.Models;
using IncognitoPAS.Services;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Controllers.Api;

[Authorize(Roles = "ModuleLeader")]
[Route("api/moduleleader")]
public class ModuleLeaderApiController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IResearchAreaService _researchAreaService;
    private readonly IUserManagementService _userManagementService;
    private readonly IMatchingService _matchingService;

    public ModuleLeaderApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IResearchAreaService researchAreaService,
        IUserManagementService userManagementService,
        IMatchingService matchingService)
    {
        _context = context;
        _userManager = userManager;
        _researchAreaService = researchAreaService;
        _userManagementService = userManagementService;
        _matchingService = matchingService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ModuleLeaderDashboardResponseDto>> GetDashboard()
    {
        var students = await _userManagementService.GetUsersByRoleAsync("Student");
        var supervisors = await _userManagementService.GetUsersByRoleAsync("Supervisor");

        var confirmedMatches = await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.Student)
            .Include(m => m.Supervisor)
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.ResearchArea)
            .Where(m => m.IsConfirmed)
            .OrderByDescending(m => m.ConfirmedAt)
            .ToListAsync();

        return Ok(new ModuleLeaderDashboardResponseDto
        {
            TotalStudents = students.Count(),
            TotalSupervisors = supervisors.Count(),
            TotalProposals = await _context.Proposals.CountAsync(),
            TotalMatches = confirmedMatches.Count,
            RecentMatches = confirmedMatches.Take(10).Select(m => new MatchOverviewDto
            {
                MatchId = m.Id,
                StudentName = m.Proposal?.Student?.FullName ?? string.Empty,
                StudentEmail = m.Proposal?.Student?.Email ?? string.Empty,
                SupervisorName = m.Supervisor?.FullName ?? string.Empty,
                SupervisorEmail = m.Supervisor?.Email ?? string.Empty,
                ProjectTitle = m.Proposal?.Title ?? string.Empty,
                ResearchArea = m.Proposal?.ResearchArea?.Name ?? string.Empty,
                Status = m.Proposal?.Status.ToString() ?? string.Empty,
                IsConfirmed = m.IsConfirmed,
                ConfirmedAt = m.ConfirmedAt
            }).ToList()
        });
    }

    [HttpGet("matches")]
    public async Task<IActionResult> GetMatches([FromQuery] string? status = null, [FromQuery] string? search = null)
    {
        var query = _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.Student)
            .Include(m => m.Supervisor)
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.ResearchArea)
            .Where(m => m.IsConfirmed)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProposalStatus>(status, out var statusFilter))
        {
            query = query.Where(m => m.Proposal != null && m.Proposal.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                (m.Proposal != null && m.Proposal.Title.Contains(term))
                || (m.Proposal != null && m.Proposal.Student != null && m.Proposal.Student.FullName.Contains(term))
                || (m.Supervisor != null && m.Supervisor.FullName.Contains(term)));
        }

        var matches = await query
            .OrderByDescending(m => m.ConfirmedAt)
            .ToListAsync();

        var dto = matches.Select(m => new MatchOverviewDto
        {
            MatchId = m.Id,
            StudentName = m.Proposal?.Student?.FullName ?? string.Empty,
            StudentEmail = m.Proposal?.Student?.Email ?? string.Empty,
            SupervisorName = m.Supervisor?.FullName ?? string.Empty,
            SupervisorEmail = m.Supervisor?.Email ?? string.Empty,
            ProjectTitle = m.Proposal?.Title ?? string.Empty,
            ResearchArea = m.Proposal?.ResearchArea?.Name ?? string.Empty,
            Status = m.Proposal?.Status.ToString() ?? string.Empty,
            IsConfirmed = m.IsConfirmed,
            ConfirmedAt = m.ConfirmedAt
        });

        return Ok(dto);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
    {
        IEnumerable<ApplicationUser> users;

        if (!string.IsNullOrWhiteSpace(role))
        {
            users = await _userManagementService.GetUsersByRoleAsync(role);
        }
        else
        {
            var students = await _userManagementService.GetUsersByRoleAsync("Student");
            var supervisors = await _userManagementService.GetUsersByRoleAsync("Supervisor");
            users = students.Concat(supervisors);
        }

        var userDtos = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
            });
        }

        return Ok(userDtos);
    }

    [HttpGet("research-areas")]
    public async Task<IActionResult> GetResearchAreas()
    {
        var areas = await _researchAreaService.GetAllAsync();
        var dto = areas.Select(a => new ResearchAreaDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description
        });

        return Ok(dto);
    }

    [HttpPost("research-areas")]
    public async Task<IActionResult> CreateResearchArea([FromBody] ResearchAreaWriteDto request)
    {
        try
        {
            var created = await _researchAreaService.CreateAsync(new ResearchArea
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            });

            return CreatedAtAction(nameof(GetResearchAreas), new { id = created.Id }, new ResearchAreaDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpPut("research-areas/{id:int}")]
    public async Task<IActionResult> UpdateResearchArea(int id, [FromBody] ResearchAreaWriteDto request)
    {
        try
        {
            var updated = await _researchAreaService.UpdateAsync(new ResearchArea
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            });

            if (updated == null)
            {
                return NotFound(new ApiErrorDto { Message = "Research area not found." });
            }

            return Ok(new ResearchAreaDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpDelete("research-areas/{id:int}")]
    public async Task<IActionResult> DeleteResearchArea(int id)
    {
        var deleted = await _researchAreaService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ApiErrorDto { Message = "Research area not found." });
        }

        return Ok(new { message = "Research area deactivated." });
    }

    [HttpPost("reassignments")]
    public async Task<IActionResult> Reassign([FromBody] ReassignRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NewSupervisorId))
        {
            return BadRequest(new ApiErrorDto { Message = "NewSupervisorId is required." });
        }

        try
        {
            var result = await _matchingService.ReassignMatchAsync(
                request.MatchId,
                request.NewSupervisorId,
                GetCurrentUserId(),
                GetIpAddress());

            return Ok(new
            {
                matchId = result.Id,
                proposalId = result.ProposalId,
                message = "Project reassigned successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }
}

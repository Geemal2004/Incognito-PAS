using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs.Api;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Controllers.Api;

[Authorize(Roles = "SysAdmin")]
[Route("api/admin")]
public class AdminApiController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserManagementService _userManagementService;

    public AdminApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserManagementService userManagementService)
    {
        _context = context;
        _userManager = userManager;
        _userManagementService = userManagementService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponseDto>> GetDashboard()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalProposals = await _context.Proposals.CountAsync();
        var pendingProposals = await _context.Proposals.CountAsync(p => p.Status == ProposalStatus.Pending);
        var totalMatches = await _context.SupervisorMatches.CountAsync();
        var confirmedMatches = await _context.SupervisorMatches.CountAsync(m => m.IsConfirmed);

        return Ok(new AdminDashboardResponseDto
        {
            TotalUsers = totalUsers,
            TotalProposals = totalProposals,
            PendingProposals = pendingProposals,
            TotalMatches = totalMatches,
            ConfirmedMatches = confirmedMatches
        });
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetUsers([FromQuery] string? role = null)
    {
        IEnumerable<ApplicationUser> users;

        if (!string.IsNullOrWhiteSpace(role))
        {
            users = await _userManagementService.GetUsersByRoleAsync(role);
        }
        else
        {
            users = await _context.Users.ToListAsync();
        }

        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
            });
        }

        return Ok(result.OrderBy(u => u.Role).ThenBy(u => u.FullName));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            var user = await _userManagementService.CreateAccountAsync(
                request.Role,
                request.Email,
                request.Password,
                request.FullName);

            var roles = await _userManager.GetRolesAsync(user);
            return Created($"/api/admin/users/{user.Id}", new UserSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? request.Role,
                IsActive = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpPatch("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        var deactivated = await _userManagementService.DeactivateAccountAsync(id);
        if (!deactivated)
        {
            return NotFound(new ApiErrorDto { Message = "User not found." });
        }

        return Ok(new { message = "User deactivated successfully." });
    }

    [HttpGet("migrations")]
    public async Task<IActionResult> GetMigrationStatus()
    {
        var migrations = await _context.Database
            .SqlQueryRaw<string>("SELECT MigrationId FROM __EFMigrationsHistory")
            .ToListAsync();

        return Ok(new { migrations });
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                ActorUserId = a.ActorUserId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return Ok(new PagedResult<AuditLogDto>
        {
            Page = safePage,
            PageSize = safePageSize,
            Total = total,
            Items = logs
        });
    }
}

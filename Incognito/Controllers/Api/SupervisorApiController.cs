using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs.Api;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Controllers.Api;

[Authorize(Roles = "Supervisor")]
[Route("api/supervisor")]
public class SupervisorApiController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IProposalService _proposalService;
    private readonly IMatchingService _matchingService;
    private readonly IExpertiseService _expertiseService;
    private readonly IResearchAreaService _researchAreaService;

    public SupervisorApiController(
        ApplicationDbContext context,
        IProposalService proposalService,
        IMatchingService matchingService,
        IExpertiseService expertiseService,
        IResearchAreaService researchAreaService)
    {
        _context = context;
        _proposalService = proposalService;
        _matchingService = matchingService;
        _expertiseService = expertiseService;
        _researchAreaService = researchAreaService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<SupervisorDashboardDto>> GetDashboard()
    {
        var supervisorId = GetCurrentUserId();

        var interests = (await _matchingService.GetSupervisorInterestsAsync(supervisorId)).ToList();
        var confirmed = (await _matchingService.GetSupervisorConfirmedMatchesAsync(supervisorId)).ToList();
        var expertise = (await _expertiseService.GetSupervisorExpertiseAsync(supervisorId)).ToList();

        var dto = new SupervisorDashboardDto
        {
            TotalInterests = interests.Count,
            ConfirmedMatches = confirmed.Count,
            ExpertiseAreas = expertise.Count,
            Expertise = expertise.Select(a => new ResearchAreaDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description
            }).ToList(),
            Interests = interests.Select(i => new SupervisorInterestDto
            {
                MatchId = i.Id,
                ProposalId = i.ProposalId,
                Title = i.Proposal?.Title ?? string.Empty,
                Abstract = i.Proposal?.Abstract ?? string.Empty,
                TechnicalStack = i.Proposal?.TechnicalStack,
                ResearchAreaName = i.Proposal?.ResearchArea?.Name ?? string.Empty,
                CreatedAt = i.Proposal?.CreatedAt ?? DateTime.UtcNow
            }).ToList(),
            Confirmed = confirmed.Select(c => new SupervisorConfirmedMatchDto
            {
                MatchId = c.Id,
                ProposalId = c.ProposalId,
                StudentName = c.Proposal?.Student?.FullName ?? string.Empty,
                StudentEmail = c.Proposal?.Student?.Email ?? string.Empty,
                IsGroupProject = c.Proposal?.ProjectGroupId.HasValue == true,
                ProjectGroupName = c.Proposal?.ProjectGroup?.Name,
                GroupMembers = c.Proposal?.ProjectGroup?.Members
                    .OrderBy(m => m.Student?.FullName ?? string.Empty)
                    .Select(m => m.Student?.FullName ?? "Unknown Student")
                    .ToList() ?? new List<string>(),
                ProjectTitle = c.Proposal?.Title ?? string.Empty,
                ConfirmedAt = c.ConfirmedAt
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpGet("browse")]
    public async Task<ActionResult<PagedResult<IncognitoPAS.DTOs.BlindProposalDto>>> Browse([FromQuery] BrowseQueryDto query)
    {
        // DECISION: Keep blind DTO projection in service as security boundary. Filtering happens post-projection.
        var proposals = (await _proposalService.GetAnonymousProposalsForSupervisorAsync(GetCurrentUserId())).ToList();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            proposals = proposals.Where(p =>
                p.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Abstract.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (p.TechnicalStack != null && p.TechnicalStack.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.ResearchArea))
        {
            proposals = proposals.Where(p => p.ResearchAreaName.Equals(query.ResearchArea, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        proposals = query.Sort.ToLowerInvariant() switch
        {
            "title" => proposals.OrderBy(p => p.Title).ToList(),
            _ => proposals.OrderByDescending(p => p.CreatedAt).ToList()
        };

        var safePage = query.Page < 1 ? 1 : query.Page;
        var safePageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var total = proposals.Count;
        var pageItems = proposals
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return Ok(new PagedResult<IncognitoPAS.DTOs.BlindProposalDto>
        {
            Page = safePage,
            PageSize = safePageSize,
            Total = total,
            Items = pageItems
        });
    }

    [HttpPost("interests/{proposalId:int}")]
    public async Task<IActionResult> ExpressInterest(int proposalId)
    {
        try
        {
            var match = await _matchingService.ExpressInterestAsync(GetCurrentUserId(), proposalId, GetIpAddress());
            return Ok(new { matchId = match.Id, message = "Interest expressed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpPost("matches/{matchId:int}/confirm")]
    public async Task<IActionResult> ConfirmMatch(int matchId, [FromBody] ConfirmMatchRequestDto request)
    {
        if (!request.Confirmed)
        {
            return BadRequest(new ApiErrorDto { Message = "Confirmation is required." });
        }

        var supervisorId = GetCurrentUserId();
        var match = await _context.SupervisorMatches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == matchId && m.SupervisorId == supervisorId);

        if (match == null)
        {
            return NotFound(new ApiErrorDto { Message = "Match not found." });
        }

        try
        {
            var confirmed = await _matchingService.ConfirmMatchAsync(supervisorId, match.ProposalId, GetIpAddress());
            return Ok(new
            {
                matchId = confirmed.Id,
                proposalId = confirmed.ProposalId,
                isRevealed = confirmed.IsRevealed,
                message = "Match confirmed and identities revealed."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpGet("matches")]
    public async Task<ActionResult<SupervisorMatchListDto>> GetMatches()
    {
        var supervisorId = GetCurrentUserId();
        var interests = (await _matchingService.GetSupervisorInterestsAsync(supervisorId)).ToList();
        var confirmed = (await _matchingService.GetSupervisorConfirmedMatchesAsync(supervisorId)).ToList();

        return Ok(new SupervisorMatchListDto
        {
            Interests = interests.Select(i => new SupervisorInterestDto
            {
                MatchId = i.Id,
                ProposalId = i.ProposalId,
                Title = i.Proposal?.Title ?? string.Empty,
                Abstract = i.Proposal?.Abstract ?? string.Empty,
                TechnicalStack = i.Proposal?.TechnicalStack,
                ResearchAreaName = i.Proposal?.ResearchArea?.Name ?? string.Empty,
                CreatedAt = i.Proposal?.CreatedAt ?? DateTime.UtcNow
            }).ToList(),
            Confirmed = confirmed.Select(c => new SupervisorConfirmedMatchDto
            {
                MatchId = c.Id,
                ProposalId = c.ProposalId,
                StudentName = c.Proposal?.Student?.FullName ?? string.Empty,
                StudentEmail = c.Proposal?.Student?.Email ?? string.Empty,
                IsGroupProject = c.Proposal?.ProjectGroupId.HasValue == true,
                ProjectGroupName = c.Proposal?.ProjectGroup?.Name,
                GroupMembers = c.Proposal?.ProjectGroup?.Members
                    .OrderBy(m => m.Student?.FullName ?? string.Empty)
                    .Select(m => m.Student?.FullName ?? "Unknown Student")
                    .ToList() ?? new List<string>(),
                ProjectTitle = c.Proposal?.Title ?? string.Empty,
                ConfirmedAt = c.ConfirmedAt
            }).ToList()
        });
    }

    [HttpGet("expertise")]
    public async Task<IActionResult> GetExpertise()
    {
        var expertise = await _expertiseService.GetSupervisorExpertiseAsync(GetCurrentUserId());
        var result = expertise.Select(a => new ResearchAreaDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description
        });

        return Ok(result);
    }

    [HttpPut("expertise")]
    public async Task<IActionResult> UpdateExpertise([FromBody] ExpertiseUpdateDto request)
    {
        await _expertiseService.SetSupervisorExpertiseAsync(GetCurrentUserId(), request.AreaIds);
        return Ok(new { message = "Expertise updated." });
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
}

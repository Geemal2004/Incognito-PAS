using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public MatchingService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<SupervisorMatch> ExpressInterestAsync(string supervisorId, int proposalId, string? ipAddress = null)
    {
        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (proposal.Status != ProposalStatus.Pending && proposal.Status != ProposalStatus.UnderReview)
        {
            throw new InvalidOperationException("Cannot express interest in this proposal.");
        }

        // Check if already expressed
        var existing = await _context.SupervisorMatches
            .FirstOrDefaultAsync(m => m.ProposalId == proposalId && m.SupervisorId == supervisorId);
        if (existing != null)
        {
            throw new InvalidOperationException("Interest already expressed for this proposal.");
        }

        var match = new SupervisorMatch
        {
            ProposalId = proposalId,
            SupervisorId = supervisorId,
            IsConfirmed = false,
            IsRevealed = false
        };

        _context.SupervisorMatches.Add(match);

        if (proposal.Status == ProposalStatus.Pending)
        {
            proposal.Status = ProposalStatus.UnderReview;
        }

        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(
            "InterestExpressed",
            nameof(SupervisorMatch),
            match.Id.ToString(),
            supervisorId,
            new { ProposalStatus = proposal.Status.ToString() },
            new { ProposalStatus = ProposalStatus.UnderReview.ToString() },
            ipAddress);

        return match;
    }

    public async Task<SupervisorMatch> ConfirmMatchAsync(string supervisorId, int proposalId, string? ipAddress = null)
    {
        var match = await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .FirstOrDefaultAsync(m => m.ProposalId == proposalId && m.SupervisorId == supervisorId);

        if (match == null)
        {
            throw new InvalidOperationException("No interest found for this proposal.");
        }

        if (match.IsConfirmed)
        {
            throw new InvalidOperationException("Match already confirmed.");
        }

        if (match.Proposal == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (match.Proposal.Status == ProposalStatus.Matched)
        {
            throw new InvalidOperationException("Proposal already matched with another supervisor.");
        }

        // Snapshot old values for audit
        var oldProposalStatus = match.Proposal.Status;

        // CONFIRM THE MATCH AND REVEAL IDENTITIES
        match.IsConfirmed = true;
        match.IsRevealed = true;  // Identity reveal happens here
        match.ConfirmedAt = DateTime.UtcNow;

        match.Proposal.Status = ProposalStatus.Matched;
        match.Proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log - match confirmed and identity revealed
        await _auditService.LogAsync(
            "MatchConfirmed",
            nameof(SupervisorMatch),
            match.Id.ToString(),
            supervisorId,
            new { IsConfirmed = false, IsRevealed = false, ProposalStatus = oldProposalStatus.ToString() },
            new { IsConfirmed = true, IsRevealed = true, ProposalStatus = ProposalStatus.Matched.ToString() },
            ipAddress);

        await _auditService.LogAsync(
            "IdentityRevealed",
            nameof(SupervisorMatch),
            match.Id.ToString(),
            supervisorId,
            null,
            new { RevealedTo = "Both parties" },
            ipAddress);

        return match;
    }

    public async Task<SupervisorMatch?> GetRevealedMatchForStudentAsync(int proposalId)
    {
        var match = await _context.SupervisorMatches
            .Include(m => m.Supervisor)
            .Include(m => m.Proposal)
            .FirstOrDefaultAsync(m => m.ProposalId == proposalId && m.IsRevealed);

        return match;
    }

    public async Task<SupervisorMatch?> GetRevealedMatchForSupervisorAsync(int matchId)
    {
        var match = await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.Student)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.IsRevealed);

        return match;
    }

    public async Task<IEnumerable<SupervisorMatch>> GetSupervisorInterestsAsync(string supervisorId)
    {
        return await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.ResearchArea)
            .Where(m => m.SupervisorId == supervisorId && !m.IsConfirmed)
            .OrderByDescending(m => m.Proposal!.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupervisorMatch>> GetSupervisorConfirmedMatchesAsync(string supervisorId)
    {
        return await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.Student)
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.ProjectGroup)
            .ThenInclude(g => g!.Members)
            .ThenInclude(member => member.Student)
            .Where(m => m.SupervisorId == supervisorId && m.IsConfirmed)
            .OrderByDescending(m => m.ConfirmedAt)
            .ToListAsync();
    }

    // FULL REASSIGNMENT WORKFLOW - implemented at service layer
    public async Task<SupervisorMatch> ReassignMatchAsync(int matchId, string newSupervisorId, string actorUserId, string? ipAddress = null)
    {
        var existingMatch = await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (existingMatch == null)
        {
            throw new InvalidOperationException("Match not found.");
        }

        if (!existingMatch.IsConfirmed)
        {
            throw new InvalidOperationException("Cannot reassign an unconfirmed match.");
        }

        var newSupervisor = await _context.Users.FindAsync(newSupervisorId);
        if (newSupervisor == null)
        {
            throw new InvalidOperationException("New supervisor not found.");
        }

        // Check new supervisor isn't already matched to this proposal
        var conflictingMatch = await _context.SupervisorMatches
            .FirstOrDefaultAsync(m => m.ProposalId == existingMatch.ProposalId && m.SupervisorId == newSupervisorId);
        if (conflictingMatch != null)
        {
            throw new InvalidOperationException("New supervisor already has an interest in this proposal.");
        }

        // Snapshot old values
        var oldSupervisorId = existingMatch.SupervisorId;
        var oldMatchId = existingMatch.Id;

        // Create new match record for new supervisor (unconfirmed)
        var newMatch = new SupervisorMatch
        {
            ProposalId = existingMatch.ProposalId,
            SupervisorId = newSupervisorId,
            IsConfirmed = false,
            IsRevealed = false
        };

        _context.SupervisorMatches.Add(newMatch);

        // Update proposal status back to UnderReview
        if (existingMatch.Proposal != null)
        {
            existingMatch.Proposal.Status = ProposalStatus.UnderReview;
            existingMatch.Proposal.UpdatedAt = DateTime.UtcNow;
        }

        // Remove old match
        _context.SupervisorMatches.Remove(existingMatch);

        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(
            "ProposalReassigned",
            nameof(ProjectProposal),
            existingMatch.ProposalId.ToString(),
            actorUserId,
            new { OldSupervisorId = oldSupervisorId, MatchId = oldMatchId },
            new { NewSupervisorId = newSupervisorId, NewMatchId = newMatch.Id },
            ipAddress);

        return newMatch;
    }
}

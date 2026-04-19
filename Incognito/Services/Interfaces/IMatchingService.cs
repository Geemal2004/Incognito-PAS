using IncognitoPAS.Models;

namespace IncognitoPAS.Services.Interfaces;

public interface IMatchingService
{
    Task<SupervisorMatch> ExpressInterestAsync(string supervisorId, int proposalId, string? ipAddress = null);
    Task<SupervisorMatch> ConfirmMatchAsync(string supervisorId, int proposalId, string? ipAddress = null);
    Task<SupervisorMatch?> GetRevealedMatchForStudentAsync(int proposalId);
    Task<SupervisorMatch?> GetRevealedMatchForSupervisorAsync(int matchId);
    Task<IEnumerable<SupervisorMatch>> GetSupervisorInterestsAsync(string supervisorId);
    Task<IEnumerable<SupervisorMatch>> GetSupervisorConfirmedMatchesAsync(string supervisorId);
    Task<SupervisorMatch> ReassignMatchAsync(int matchId, string newSupervisorId, string actorUserId, string? ipAddress = null);
}

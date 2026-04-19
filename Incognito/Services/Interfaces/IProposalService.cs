using IncognitoPAS.DTOs;
using IncognitoPAS.Models;

namespace IncognitoPAS.Services.Interfaces;

public interface IProposalService
{
    Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal, string studentId);
    Task<IEnumerable<StudentProposalDto>> GetProposalsByStudentAsync(string studentId);
    Task<ProjectProposal?> UpdateProposalAsync(ProjectProposal proposal, string studentId);
    Task WithdrawProposalAsync(int proposalId, string studentId, string? ipAddress = null);
    Task<IEnumerable<BlindProposalDto>> GetAnonymousProposalsForSupervisorAsync(string supervisorId);
    Task<ProjectGroupDto> CreateProjectGroupAsync(string leaderId, string name, IEnumerable<string> memberStudentIds);
    Task<IEnumerable<ProjectGroupDto>> GetProjectGroupsForStudentAsync(string studentId);
    Task<IEnumerable<StudentPeerDto>> GetStudentPeersAsync(string studentId);
}

using IncognitoPAS.Models;

namespace IncognitoPAS.Services.Interfaces;

public interface IExpertiseService
{
    Task<IEnumerable<ResearchArea>> GetSupervisorExpertiseAsync(string supervisorId);
    Task SetSupervisorExpertiseAsync(string supervisorId, IEnumerable<int> areaIds);
}
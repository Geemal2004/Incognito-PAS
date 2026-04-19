using IncognitoPAS.Models;

namespace IncognitoPAS.Services.Interfaces;

public interface IResearchAreaService
{
    Task<IEnumerable<ResearchArea>> GetAllAsync();
    Task<ResearchArea?> GetByIdAsync(int id);
    Task<ResearchArea> CreateAsync(ResearchArea area);
    Task<ResearchArea?> UpdateAsync(ResearchArea area);
    Task<bool> DeleteAsync(int id);
}
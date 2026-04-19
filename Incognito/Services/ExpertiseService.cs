using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class ExpertiseService : IExpertiseService
{
    private readonly ApplicationDbContext _context;

    public ExpertiseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ResearchArea>> GetSupervisorExpertiseAsync(string supervisorId)
    {
        return await _context.SupervisorExpertises
            .Include(se => se.ResearchArea)
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchArea!)
            .ToListAsync();
    }

    public async Task SetSupervisorExpertiseAsync(string supervisorId, IEnumerable<int> areaIds)
    {
        var supervisor = await _context.Users.FindAsync(supervisorId);
        if (supervisor == null)
        {
            throw new InvalidOperationException("User not found.");
        }
        // Note: Role validation should be done via UserManager at controller level
        // This service method expects the caller to have already validated the user is a Supervisor

        // Remove existing expertise
        var existing = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .ToListAsync();
        _context.SupervisorExpertises.RemoveRange(existing);

        // Add new expertise
        foreach (var areaId in areaIds)
        {
            var area = await _context.ResearchAreas.FindAsync(areaId);
            if (area != null && area.IsActive)
            {
                _context.SupervisorExpertises.Add(new SupervisorExpertise
                {
                    SupervisorId = supervisorId,
                    ResearchAreaId = areaId
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
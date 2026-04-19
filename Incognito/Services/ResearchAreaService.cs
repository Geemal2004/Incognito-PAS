using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class ResearchAreaService : IResearchAreaService
{
    private readonly ApplicationDbContext _context;

    public ResearchAreaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ResearchArea>> GetAllAsync()
    {
        return await _context.ResearchAreas
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<ResearchArea?> GetByIdAsync(int id)
    {
        return await _context.ResearchAreas.FindAsync(id);
    }

    public async Task<ResearchArea> CreateAsync(ResearchArea area)
    {
        if (string.IsNullOrWhiteSpace(area.Name))
        {
            throw new ValidationException("Research area name is required.");
        }

        var existing = await _context.ResearchAreas
            .FirstOrDefaultAsync(r => r.Name == area.Name);
        if (existing != null)
        {
            throw new InvalidOperationException("Research area already exists.");
        }

        area.IsActive = true;
        _context.ResearchAreas.Add(area);
        await _context.SaveChangesAsync();

        return area;
    }

    public async Task<ResearchArea?> UpdateAsync(ResearchArea area)
    {
        var existing = await _context.ResearchAreas.FindAsync(area.Id);
        if (existing == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(area.Name))
        {
            throw new ValidationException("Research area name is required.");
        }

        existing.Name = area.Name;
        existing.Description = area.Description;
        existing.IsActive = area.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var area = await _context.ResearchAreas.FindAsync(id);
        if (area == null)
        {
            return false;
        }

        // Soft delete
        area.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
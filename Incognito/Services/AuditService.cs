using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using IncognitoPAS.Data;
using IncognitoPAS.Models;

namespace IncognitoPAS.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, string? actorUserId, object? oldValues = null, object? newValues = null, string? ipAddress = null);
    Task<IEnumerable<AuditLog>> GetLogsForEntityAsync(string entityType, string entityId);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string? actorUserId,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            ActorUserId = actorUserId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsForEntityAsync(string entityType, string entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}

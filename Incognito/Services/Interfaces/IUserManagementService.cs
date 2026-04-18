using IncognitoPAS.Models;

namespace IncognitoPAS.Services.Interfaces;

public interface IUserManagementService
{
    Task<ApplicationUser> CreateAccountAsync(string role, string email, string password, string fullName);
    Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role);
    Task<bool> DeactivateAccountAsync(string userId);
    Task<ApplicationUser?> GetByIdAsync(string userId);
}
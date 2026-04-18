using Microsoft.AspNetCore.Identity;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser> CreateAccountAsync(string role, string email, string password, string fullName)
    {
        if (role != "Student" && role != "Supervisor")
        {
            throw new InvalidOperationException("Account can only be created for Student or Supervisor roles.");
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to create user: " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);
        return user;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users;
    }

    public async Task<bool> DeactivateAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return true;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }
}

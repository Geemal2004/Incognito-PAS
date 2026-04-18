using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using IncognitoPAS.DTOs;
using IncognitoPAS.Models;
using IncognitoPAS.Services.Interfaces;

namespace IncognitoPAS.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto model)
    {
        if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Password and confirm password do not match."
            };
        }

        // Validate role
        if (model.Role != "Student" && model.Role != "Supervisor")
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid role. Only Student or Supervisor registration is allowed."
            };
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, model.Role);

        // Generate token
        var token = await GenerateJwtTokenAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = model.Role,
                ProfileImageUrl = user.ProfileImageUrl
            }
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var token = await GenerateJwtTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Unknown",
                ProfileImageUrl = user.ProfileImageUrl
            }
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return await ToUserDtoAsync(user);
    }

    public async Task<UserDto?> UpdateProfileImageAsync(string userId, string profileImageUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        user.ProfileImageUrl = profileImageUrl.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return null;
        }

        return await ToUserDtoAsync(user);
    }

    private async Task<UserDto> ToUserDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Unknown",
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "IncognitoPAS_SecretKey_ForDevelopment_2026!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "IncognitoPAS";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "IncognitoPAS";

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("name", user.FullName)
        };

        claims.AddRange(roleClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
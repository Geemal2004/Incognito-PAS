using System.ComponentModel.DataAnnotations;

namespace IncognitoPAS.DTOs.Api;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Student|Supervisor)$", ErrorMessage = "Only Student or Supervisor can self-register.")]
    public string Role { get; set; } = "Student";
}

public class UpdateProfileImageRequestDto
{
    [Required]
    [Url]
    [MaxLength(500)]
    public string ProfileImageUrl { get; set; } = string.Empty;
}

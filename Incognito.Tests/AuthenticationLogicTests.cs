using IncognitoPAS.Models;
using IncognitoPAS.Services;
using IncognitoPAS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Incognito.Tests;

// UNIT TESTS: uses Moq to isolate AuthService login logic from real infrastructure.
public class AuthenticationLogicTests
{
    [Fact]
    public async Task Authentication_ShouldSucceed_WithCorrectCredentials()
    {
        // This test verifies login works with valid email and password.
        var userManager = TestHelpers.CreateUserManagerMock();
        var signInManager = TestHelpers.CreateSignInManagerMock(userManager.Object);

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "student@test.com",
            UserName = "student@test.com",
            FullName = "Student One"
        };

        userManager.Setup(x => x.FindByEmailAsync("student@test.com")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });
        signInManager.Setup(x => x.CheckPasswordSignInAsync(user, "Correct123!", false))
            .ReturnsAsync(SignInResult.Success);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Development_Test_Key_12345678901234567890",
                ["Jwt:Issuer"] = "BlindMatchPAS",
                ["Jwt:Audience"] = "BlindMatchPAS"
            })
            .Build();

        var service = new AuthService(userManager.Object, signInManager.Object, config);
        var result = await service.LoginAsync(new LoginDto { Email = "student@test.com", Password = "Correct123!" });

        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("Student", result.User!.Role);

        userManager.Verify(x => x.FindByEmailAsync("student@test.com"), Times.Once);
        signInManager.Verify(x => x.CheckPasswordSignInAsync(user, "Correct123!", false), Times.Once);
        // AuthService reads roles once for token generation and once for response mapping.
        userManager.Verify(x => x.GetRolesAsync(user), Times.Exactly(2));
    }

    [Fact]
    public async Task Authentication_ShouldFail_WithUnknownEmail()
    {
        // This test validates login failure when the email does not exist.
        var userManager = TestHelpers.CreateUserManagerMock();
        var signInManager = TestHelpers.CreateSignInManagerMock(userManager.Object);

        userManager.Setup(x => x.FindByEmailAsync("missing@test.com")).ReturnsAsync((ApplicationUser?)null);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Development_Test_Key_12345678901234567890"
            })
            .Build();

        var service = new AuthService(userManager.Object, signInManager.Object, config);
        var result = await service.LoginAsync(new LoginDto { Email = "missing@test.com", Password = "Wrong123!" });

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);

        userManager.Verify(x => x.FindByEmailAsync("missing@test.com"), Times.Once);
        signInManager.Verify(
            x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
        userManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Authentication_ShouldFail_WithIncorrectPassword()
    {
        // This test validates login failure when password is incorrect.
        var userManager = TestHelpers.CreateUserManagerMock();
        var signInManager = TestHelpers.CreateSignInManagerMock(userManager.Object);

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "student@test.com",
            UserName = "student@test.com",
            FullName = "Student One"
        };

        userManager.Setup(x => x.FindByEmailAsync("student@test.com")).ReturnsAsync(user);
        signInManager.Setup(x => x.CheckPasswordSignInAsync(user, "WrongPassword!", false))
            .ReturnsAsync(SignInResult.Failed);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Development_Test_Key_12345678901234567890"
            })
            .Build();

        var service = new AuthService(userManager.Object, signInManager.Object, config);
        var result = await service.LoginAsync(new LoginDto { Email = "student@test.com", Password = "WrongPassword!" });

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);

        userManager.Verify(x => x.FindByEmailAsync("student@test.com"), Times.Once);
        signInManager.Verify(x => x.CheckPasswordSignInAsync(user, "WrongPassword!", false), Times.Once);
        userManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Authentication_ShouldNotCheckPassword_WhenEmailDoesNotExist()
    {
        var userManager = TestHelpers.CreateUserManagerMock();
        var signInManager = TestHelpers.CreateSignInManagerMock(userManager.Object);

        userManager.Setup(x => x.FindByEmailAsync("unknown@test.com")).ReturnsAsync((ApplicationUser?)null);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Development_Test_Key_12345678901234567890"
            })
            .Build();

        var service = new AuthService(userManager.Object, signInManager.Object, config);

        var result = await service.LoginAsync(new LoginDto { Email = "unknown@test.com", Password = "Anything123!" });

        Assert.False(result.Success);
        signInManager.Verify(
            x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }
}
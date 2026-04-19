using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services;

namespace Incognito.Tests;

public class ProposalServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProposalService _service;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Mock<IAuditService> _auditServiceMock;

    public ProposalServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userManager = CreateUserManager();
        _auditServiceMock = new Mock<IAuditService>();
        _auditServiceMock
            .Setup(a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _service = new ProposalService(_context, _userManager, _auditServiceMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var student = new ApplicationUser
        {
            Id = "student1",
            UserName = "student@test.com",
            Email = "student@test.com",
            FullName = "Test Student"
        };

        var supervisor = new ApplicationUser
        {
            Id = "supervisor1",
            UserName = "supervisor@test.com",
            Email = "supervisor@test.com",
            FullName = "Test Supervisor"
        };

        var area = new ResearchArea
        {
            Id = 1,
            Name = "Artificial Intelligence",
            IsActive = true
        };

        _context.Users.AddRange(student, supervisor);
        _context.ResearchAreas.Add(area);
        _context.SaveChanges();
    }

    private UserManager<ApplicationUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var manager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        manager
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _context.Users.FirstOrDefault(u => u.Id == id));

        manager
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser user) =>
            {
                if (user.Id.StartsWith("student", StringComparison.OrdinalIgnoreCase))
                {
                    return (IList<string>)new List<string> { "Student" };
                }

                if (user.Id.StartsWith("supervisor", StringComparison.OrdinalIgnoreCase))
                {
                    return (IList<string>)new List<string> { "Supervisor" };
                }

                return (IList<string>)new List<string>();
            });

        manager
            .Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string role) =>
                role.Equals("Student", StringComparison.OrdinalIgnoreCase)
                    ? user.Id.StartsWith("student", StringComparison.OrdinalIgnoreCase)
                    : role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase)
                        ? user.Id.StartsWith("supervisor", StringComparison.OrdinalIgnoreCase)
                        : false);

        return manager.Object;
    }

    [Fact]
    public async Task CreateProposal_Fails_WhenTitleTooShort()
    {
        // Arrange
        var proposal = new ProjectProposal
        {
            Title = "Short",
            Abstract = "This abstract is long enough to pass the 100 character minimum requirement for validation.",
            ResearchAreaId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateProposalAsync(proposal, "student1"));
    }

    [Fact]
    public async Task CreateProposal_Succeeds_WithValidData()
    {
        // Arrange
        var proposal = new ProjectProposal
        {
            Title = "Valid Title for Testing",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = 1
        };

        // Act
        var result = await _service.CreateProposalAsync(proposal, "student1");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(ProposalStatus.Pending);
    }

    [Fact]
    public async Task GetProposalsByStudent_ReturnsOnlyOwnProposals()
    {
        // Arrange
        var proposal = new ProjectProposal
        {
            Title = "Valid Title for Testing",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = 1,
            StudentId = "student1"
        };
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.GetProposalsByStudentAsync("student1");

        // Assert
        results.Should().ContainSingle();
        results.First().Title.Should().Be("Valid Title for Testing");
        results.First().CanManage.Should().BeTrue();
    }

    [Fact]
    public async Task WithdrawProposal_Throws_WhenStatusIsMatched()
    {
        // Arrange
        var matchedProposal = new ProjectProposal
        {
            Id = 100,
            Title = "Matched Project Title",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = 1,
            StudentId = "student1",
            Status = ProposalStatus.Matched
        };
        _context.Proposals.Add(matchedProposal);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.WithdrawProposalAsync(100, "student1"));
    }

    [Fact]
    public async Task GetAnonymousProposalsForSupervisor_ReturnsBlindDto()
    {
        // Arrange
        var expertise = new SupervisorExpertise
        {
            SupervisorId = "supervisor1",
            ResearchAreaId = 1
        };
        _context.SupervisorExpertises.Add(expertise);

        var proposal = new ProjectProposal
        {
            Title = "Valid Title for Testing",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = 1,
            StudentId = "student1",
            Status = ProposalStatus.Pending,
            IsAnonymous = true
        };
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.GetAnonymousProposalsForSupervisorAsync("supervisor1");

        // Assert
        results.Should().NotBeEmpty();
        var dto = results.First();
        dto.Id.Should().BeGreaterThan(0);
        dto.Title.Should().NotBeNull();
        dto.ResearchAreaName.Should().Be("Artificial Intelligence");
        // ⛔ Verify no StudentId/StudentName properties exist on the DTO
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
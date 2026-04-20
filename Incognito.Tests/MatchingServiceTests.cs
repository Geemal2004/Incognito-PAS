using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services;

namespace Incognito.Tests;

// SERVICE TESTS (unit-style with persistence):
// Uses EF Core InMemory for workflow state and Moq for audit dependency verification.
public class MatchingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MatchingService _service;
    private readonly Mock<IAuditService> _auditServiceMock;

    public MatchingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
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

        _service = new MatchingService(_context, _auditServiceMock.Object);

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

        var proposal = new ProjectProposal
        {
            Id = 1,
            Title = "Machine Learning for Image Recognition",
            Abstract = "This is a test abstract that is long enough to pass validation checks for the system.",
            ResearchAreaId = 1,
            StudentId = "student1",
            Status = ProposalStatus.Pending,
            IsAnonymous = true
        };

        _context.Users.AddRange(student, supervisor);
        _context.ResearchAreas.Add(area);
        _context.Proposals.Add(proposal);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ExpressInterest_CreatesMatch_WithIsConfirmedFalse()
    {
        // Act
        var result = await _service.ExpressInterestAsync("supervisor1", 1);

        // Assert
        result.Should().NotBeNull();
        result.IsConfirmed.Should().BeFalse();
        result.IsRevealed.Should().BeFalse();

        _auditServiceMock.Verify(
            a => a.LogAsync(
                "InterestExpressed",
                nameof(SupervisorMatch),
                It.IsAny<string>(),
                "supervisor1",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmMatch_SetsIsRevealed_AndStatusMatched()
    {
        // Arrange
        await _service.ExpressInterestAsync("supervisor1", 1);

        // Act
        var result = await _service.ConfirmMatchAsync("supervisor1", 1);

        // Assert
        result.Should().NotBeNull();
        result.IsConfirmed.Should().BeTrue();
        result.IsRevealed.Should().BeTrue(); // Identity reveal happens here
        result.ConfirmedAt.Should().NotBeNull();

        var proposal = await _context.Proposals.FindAsync(1);
        proposal!.Status.Should().Be(ProposalStatus.Matched);

        _auditServiceMock.Verify(
            a => a.LogAsync(
                "MatchConfirmed",
                nameof(SupervisorMatch),
                It.IsAny<string>(),
                "supervisor1",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Once);
        _auditServiceMock.Verify(
            a => a.LogAsync(
                "IdentityRevealed",
                nameof(SupervisorMatch),
                It.IsAny<string>(),
                "supervisor1",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmMatch_Throws_WhenAlreadyMatched()
    {
        // Arrange
        await _service.ExpressInterestAsync("supervisor1", 1);
        await _service.ConfirmMatchAsync("supervisor1", 1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfirmMatchAsync("supervisor1", 1));

        // 1 from express interest + 2 from first confirm. No additional audit on failed re-confirm.
        _auditServiceMock.Verify(
            a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task GetRevealedMatchForStudent_ReturnsMatch_WhenRevealed()
    {
        // Arrange
        await _service.ExpressInterestAsync("supervisor1", 1);
        await _service.ConfirmMatchAsync("supervisor1", 1);

        // Act
        var result = await _service.GetRevealedMatchForStudentAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.IsRevealed.Should().BeTrue();
        result.Supervisor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRevealedMatchForStudent_ReturnsNull_WhenNotRevealedYet()
    {
        // Arrange
        var supervisorExpertise = new SupervisorExpertise
        {
            SupervisorId = "supervisor1",
            ResearchAreaId = 1
        };
        _context.SupervisorExpertises.Add(supervisorExpertise);
        await _context.SaveChangesAsync();

        // Act
        var proposals = await _service.GetRevealedMatchForStudentAsync(1);

        // Assert
        // The DTO itself is tested in ProposalService tests
        proposals.Should().BeNull(); // Not revealed yet
    }

    [Fact]
    public async Task ExpressInterest_Throws_WhenAlreadyExpressed()
    {
        // Arrange
        await _service.ExpressInterestAsync("supervisor1", 1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExpressInterestAsync("supervisor1", 1));

        // No additional audit entry should be written after duplicate-interest validation fails.
        _auditServiceMock.Verify(
            a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
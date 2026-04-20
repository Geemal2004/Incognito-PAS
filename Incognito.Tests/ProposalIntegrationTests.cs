using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services;

namespace Incognito.Tests;

// INTEGRATION TESTS: end-to-end service workflow checks with EF Core InMemory state transitions and reveal behavior.
public class ProposalIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProposalService _proposalService;
    private readonly MatchingService _matchingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Mock<IAuditService> _auditServiceMock;

    public ProposalIntegrationTests()
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

        _proposalService = new ProposalService(_context, _userManager, _auditServiceMock.Object);
        _matchingService = new MatchingService(_context, _auditServiceMock.Object);

        SeedData();
    }

    private void SeedData()
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
    public async Task FullWorkflow_Submit_Interest_Confirm_BothRevealed()
    {
        // 1. Student creates proposal
        var proposal = new ProjectProposal
        {
            Title = "Machine Learning Research Project",
            Abstract = "This abstract describes a machine learning project that will revolutionize the field of artificial intelligence.",
            ResearchAreaId = 1
        };
        var created = await _proposalService.CreateProposalAsync(proposal, "student1");
        created.Status.Should().Be(ProposalStatus.Pending);

        // 2. Supervisor sets expertise and browses
        var expertise = new SupervisorExpertise
        {
            SupervisorId = "supervisor1",
            ResearchAreaId = 1
        };
        _context.SupervisorExpertises.Add(expertise);
        await _context.SaveChangesAsync();

        var blindProposals = await _proposalService.GetAnonymousProposalsForSupervisorAsync("supervisor1");
        blindProposals.Should().ContainSingle();

        // 3. Supervisor expresses interest
        var match = await _matchingService.ExpressInterestAsync("supervisor1", created.Id);
        match.IsConfirmed.Should().BeFalse();

        var updatedProposal = await _context.Proposals.FindAsync(created.Id);
        updatedProposal!.Status.Should().Be(ProposalStatus.UnderReview);

        // 4. Supervisor confirms match - TRIGGERS IDENTITY REVEAL
        var confirmedMatch = await _matchingService.ConfirmMatchAsync("supervisor1", created.Id);
        confirmedMatch.IsRevealed.Should().BeTrue();
        confirmedMatch.IsConfirmed.Should().BeTrue();

        // 5. Both parties can now see each other's info
        var studentMatch = await _matchingService.GetRevealedMatchForStudentAsync(created.Id);
        studentMatch.Should().NotBeNull();
        studentMatch!.Supervisor.Should().NotBeNull();
        studentMatch.Supervisor!.FullName.Should().Be("Test Supervisor");

        var supervisorMatch = await _matchingService.GetRevealedMatchForSupervisorAsync(confirmedMatch.Id);
        supervisorMatch.Should().NotBeNull();
        supervisorMatch!.Proposal!.Student.Should().NotBeNull();
    }

    [Fact]
    public async Task StudentCannotReadOtherStudentsProposals()
    {
        // Create another student
        var anotherStudent = new ApplicationUser
        {
            Id = "student2",
            UserName = "student2@test.com",
            Email = "student2@test.com",
            FullName = "Another Student"
        };
        _context.Users.Add(anotherStudent);
        await _context.SaveChangesAsync();

        // Create proposal for student1
        var proposal = new ProjectProposal
        {
            Title = "Student One Project",
            Abstract = "This abstract is for student one's project that should not be visible to student two in the system.",
            ResearchAreaId = 1,
            StudentId = "student1"
        };
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        // Student2 tries to get their proposals (should be empty)
        var student2Proposals = await _proposalService.GetProposalsByStudentAsync("student2");
        student2Proposals.Should().BeEmpty();

        // Student1 gets their proposals (should have one)
        var student1Proposals = await _proposalService.GetProposalsByStudentAsync("student1");
        student1Proposals.Should().ContainSingle();
        student1Proposals.First().Title.Should().Be("Student One Project");
        student1Proposals.First().CanManage.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmMatch_UpdatesProposalStatus_InDatabase()
    {
        // Create proposal
        var proposal = new ProjectProposal
        {
            Title = "Test Project for Status Update",
            Abstract = "This abstract is for testing the status update functionality when a supervisor confirms a match in the system.",
            ResearchAreaId = 1,
            StudentId = "student1",
            Status = ProposalStatus.Pending
        };
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        // Express interest
        await _matchingService.ExpressInterestAsync("supervisor1", proposal.Id);

        // Confirm match
        await _matchingService.ConfirmMatchAsync("supervisor1", proposal.Id);

        // Verify database record
        var dbProposal = await _context.Proposals.FindAsync(proposal.Id);
        dbProposal!.Status.Should().Be(ProposalStatus.Matched);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
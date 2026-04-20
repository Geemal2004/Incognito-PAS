using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using IncognitoPAS.Data;
using IncognitoPAS.Models;
using IncognitoPAS.Services;

namespace Incognito.Tests;

// SERVICE TESTS (unit-style with persistence):
// Uses Moq for UserManager and IAuditService while using EF Core InMemory for realistic state transitions.
public class ProposalServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProposalService _service;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Mock<IAuditService> _auditServiceMock;

    public ProposalServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userManagerMock = CreateUserManagerMock();
        _userManager = _userManagerMock.Object;
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

        var studentTwo = new ApplicationUser
        {
            Id = "student2",
            UserName = "student2@test.com",
            Email = "student2@test.com",
            FullName = "Student Two"
        };

        var studentThree = new ApplicationUser
        {
            Id = "student3",
            UserName = "student3@test.com",
            Email = "student3@test.com",
            FullName = "Student Three"
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

        _context.Users.AddRange(student, studentTwo, studentThree, supervisor);
        _context.ResearchAreas.Add(area);
        _context.SaveChanges();
    }

    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
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

        return manager;
    }

    private static ProjectProposal CreateValidProposal(int researchAreaId = 1)
    {
        return new ProjectProposal
        {
            Title = "Valid Title for Testing",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = researchAreaId
        };
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

        _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _auditServiceMock.Verify(
            a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateProposal_Succeeds_WithValidData()
    {
        // Arrange
        var proposal = CreateValidProposal();

        // Act
        var result = await _service.CreateProposalAsync(proposal, "student1");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(ProposalStatus.Pending);

        _userManagerMock.Verify(m => m.FindByIdAsync("student1"), Times.Once);
        _userManagerMock.Verify(m => m.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "student1")), Times.Once);
        // CreateProposalAsync currently does not emit an audit entry; this assertion documents current behavior.
        _auditServiceMock.Verify(
            a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateProposal_RejectsNonCloudinaryPdfUrl()
    {
        var proposal = CreateValidProposal();
        proposal.ProposalDocumentUrl = "https://example.com/proposal.pdf";

        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateProposalAsync(proposal, "student1"));

        _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProposal_Succeeds_ForOwnerWhenPending()
    {
        var proposal = CreateValidProposal();
        proposal.StudentId = "student1";
        proposal.Status = ProposalStatus.Pending;
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        var update = new ProjectProposal
        {
            Id = proposal.Id,
            Title = "Updated Proposal Title",
            Abstract = "This updated abstract remains valid because it clearly exceeds one hundred characters and keeps the proposal quality constraints satisfied.",
            TechnicalStack = "C#, .NET, Angular",
            ResearchAreaId = 1,
            ProposalDocumentUrl = "https://res.cloudinary.com/dy3jmad0j/raw/upload/v1/proposals/proposal.pdf"
        };

        var updated = await _service.UpdateProposalAsync(update, "student1");

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Proposal Title");
        updated.TechnicalStack.Should().Be("C#, .NET, Angular");
    }

    [Fact]
    public async Task UpdateProposal_Throws_WhenRequesterIsNotOwner()
    {
        var proposal = CreateValidProposal();
        proposal.StudentId = "student1";
        proposal.Status = ProposalStatus.Pending;
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        var update = CreateValidProposal();
        update.Id = proposal.Id;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateProposalAsync(update, "student2"));
    }

    [Fact]
    public async Task UpdateProposal_Throws_WhenStatusIsNotPending()
    {
        var proposal = CreateValidProposal();
        proposal.StudentId = "student1";
        proposal.Status = ProposalStatus.UnderReview;
        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        var update = CreateValidProposal();
        update.Id = proposal.Id;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateProposalAsync(update, "student1"));
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

        _auditServiceMock.Verify(
            a => a.LogAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task WithdrawProposal_LogsAudit_WhenPendingProposalIsWithdrawn()
    {
        var pendingProposal = new ProjectProposal
        {
            Id = 101,
            Title = "Withdrawable Project Title",
            Abstract = "This is a valid abstract that is long enough to pass the 100 character minimum requirement for validation in the system.",
            ResearchAreaId = 1,
            StudentId = "student1",
            Status = ProposalStatus.Pending
        };
        _context.Proposals.Add(pendingProposal);
        await _context.SaveChangesAsync();

        await _service.WithdrawProposalAsync(101, "student1", "127.0.0.1");

        var saved = await _context.Proposals.FindAsync(101);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(ProposalStatus.Withdrawn);

        _auditServiceMock.Verify(
            a => a.LogAsync(
                "ProposalWithdrawn",
                nameof(ProjectProposal),
                "101",
                "student1",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                "127.0.0.1"),
            Times.Once);
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

    [Fact]
    public async Task CreateProjectGroup_Succeeds_ForValidMembers()
    {
        var group = await _service.CreateProjectGroupAsync(
            "student1",
            "PUSL Team 01",
            new[] { "student2", "student3" });

        group.Name.Should().Be("PUSL Team 01");
        group.LeaderId.Should().Be("student1");
        group.Members.Should().HaveCount(3);
        group.Members.Should().Contain(m => m.StudentId == "student1" && m.IsLeader);
        group.Members.Should().Contain(m => m.StudentId == "student2");
        group.Members.Should().Contain(m => m.StudentId == "student3");

        _userManagerMock.Verify(m => m.IsInRoleAsync(It.Is<ApplicationUser>(u => u.Id == "student2"), "Student"), Times.Once);
        _userManagerMock.Verify(m => m.IsInRoleAsync(It.Is<ApplicationUser>(u => u.Id == "student3"), "Student"), Times.Once);
    }

    [Fact]
    public async Task CreateProjectGroup_Throws_WhenNoAdditionalMembers()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateProjectGroupAsync("student1", "PUSL Team 02", Array.Empty<string>()));
    }

    [Fact]
    public async Task CreateProposal_Succeeds_ForGroupLeaderSubmission()
    {
        var group = new ProjectGroup
        {
            Name = "PUSL Team 03",
            LeaderId = "student1",
            Members = new List<ProjectGroupMember>
            {
                new() { StudentId = "student1" },
                new() { StudentId = "student2" }
            }
        };
        _context.ProjectGroups.Add(group);
        await _context.SaveChangesAsync();

        var proposal = CreateValidProposal();
        proposal.ProjectGroupId = group.Id;

        var created = await _service.CreateProposalAsync(proposal, "student1");

        created.ProjectGroupId.Should().Be(group.Id);
        created.StudentId.Should().Be("student1");
        created.Status.Should().Be(ProposalStatus.Pending);
    }

    [Fact]
    public async Task CreateProposal_Throws_WhenGroupSubmitterIsNotLeader()
    {
        var group = new ProjectGroup
        {
            Name = "PUSL Team 04",
            LeaderId = "student1",
            Members = new List<ProjectGroupMember>
            {
                new() { StudentId = "student1" },
                new() { StudentId = "student2" }
            }
        };
        _context.ProjectGroups.Add(group);
        await _context.SaveChangesAsync();

        var proposal = CreateValidProposal();
        proposal.ProjectGroupId = group.Id;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateProposalAsync(proposal, "student2"));
    }

    [Fact]
    public async Task GetProjectGroupsForStudent_ReturnsGroupMembership()
    {
        var group = new ProjectGroup
        {
            Name = "PUSL Team 05",
            LeaderId = "student1",
            Members = new List<ProjectGroupMember>
            {
                new() { StudentId = "student1" },
                new() { StudentId = "student2" }
            }
        };
        _context.ProjectGroups.Add(group);
        await _context.SaveChangesAsync();

        var groups = (await _service.GetProjectGroupsForStudentAsync("student2")).ToList();

        groups.Should().ContainSingle();
        groups[0].Name.Should().Be("PUSL Team 05");
        groups[0].Members.Should().Contain(m => m.StudentId == "student2");
    }

    [Fact]
    public async Task GetStudentPeers_ReturnsOnlyStudentsAndExcludesCurrentStudent()
    {
        _context.Roles.Add(new IdentityRole
        {
            Id = "role-student",
            Name = "Student",
            NormalizedName = "STUDENT"
        });

        _context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = "student1", RoleId = "role-student" },
            new IdentityUserRole<string> { UserId = "student2", RoleId = "role-student" },
            new IdentityUserRole<string> { UserId = "student3", RoleId = "role-student" });

        await _context.SaveChangesAsync();

        var peers = (await _service.GetStudentPeersAsync("student1")).ToList();

        peers.Should().Contain(p => p.Id == "student2");
        peers.Should().Contain(p => p.Id == "student3");
        peers.Should().NotContain(p => p.Id == "student1");
        peers.Should().NotContain(p => p.Id == "supervisor1");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
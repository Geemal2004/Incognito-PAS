using IncognitoPAS.Models;
using IncognitoPAS.Services;
using Moq;

namespace Incognito.Tests;

// INTEGRATION TESTS: verifies ProposalService validation with EF Core InMemory persistence and mocked identity dependencies.
public class ProjectSubmissionValidationTests
{
    [Fact]
    public async Task ProjectSubmissionValidation_ShouldAcceptValidInput()
    {
        // This test ensures valid title and category (research area) are accepted.
        using var context = TestHelpers.CreateContext();
        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
        await context.SaveChangesAsync();

        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("student-1"))
            .ReturnsAsync(new ApplicationUser { Id = "student-1", Email = "student@test.com", FullName = "Student One" });
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Student" });

        var service = new ProposalService(context, userManager.Object, new Mock<IAuditService>().Object);

        var proposal = new ProjectProposal
        {
            Title = "Valid Project Submission Title",
            Abstract = new string('A', 120),
            ResearchAreaId = 1
        };

        var result = await service.CreateProposalAsync(proposal, "student-1");

        Assert.Equal(ProposalStatus.Pending, result.Status);
        userManager.Verify(x => x.FindByIdAsync("student-1"), Times.Once);
        userManager.Verify(x => x.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "student-1")), Times.Once);
    }

    [Fact]
    public async Task ProjectSubmissionValidation_ShouldRejectEmptyTitle()
    {
        // This test covers empty title input and expects validation failure.
        using var context = TestHelpers.CreateContext();
        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
        await context.SaveChangesAsync();

        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("student-1"))
            .ReturnsAsync(new ApplicationUser { Id = "student-1" });
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Student" });

        var service = new ProposalService(context, userManager.Object, new Mock<IAuditService>().Object);

        var proposal = new ProjectProposal
        {
            Title = "",
            Abstract = new string('A', 120),
            ResearchAreaId = 1
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateProposalAsync(proposal, "student-1"));
        userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        userManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ProjectSubmissionValidation_ShouldRejectInvalidCategory()
    {
        // This test checks invalid category by sending a missing research area id.
        using var context = TestHelpers.CreateContext();

        var userManager = TestHelpers.CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync("student-1"))
            .ReturnsAsync(new ApplicationUser { Id = "student-1" });
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Student" });

        var service = new ProposalService(context, userManager.Object, new Mock<IAuditService>().Object);

        var proposal = new ProjectProposal
        {
            Title = "Valid Title But Invalid Category",
            Abstract = new string('A', 120),
            ResearchAreaId = 999
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProposalAsync(proposal, "student-1"));
        userManager.Verify(x => x.FindByIdAsync("student-1"), Times.Once);
        userManager.Verify(x => x.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "student-1")), Times.Once);
    }
}
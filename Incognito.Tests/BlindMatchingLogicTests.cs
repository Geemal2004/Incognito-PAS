using IncognitoPAS.DTOs;
using IncognitoPAS.Models;
using IncognitoPAS.Services;
using Moq;

namespace Incognito.Tests;

public class BlindMatchingLogicTests
{
    [Fact]
    public async Task BlindMatching_ShouldHideStudentIdentityFields_BeforeSelection()
    {
        // This test verifies the blind DTO does not expose student identity fields.
        using var context = TestHelpers.CreateContext();

        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
        context.SupervisorExpertises.Add(new SupervisorExpertise
        {
            SupervisorId = "supervisor-1",
            ResearchAreaId = 1
        });
        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Blind Proposal Title",
            Abstract = new string('A', 120),
            StudentId = "student-secret",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new ProposalService(
            context,
            TestHelpers.CreateUserManagerMock().Object,
            new Mock<IAuditService>().Object);

        var result = (await service.GetAnonymousProposalsForSupervisorAsync("supervisor-1")).ToList();

        Assert.Single(result);
        var propertyNames = typeof(BlindProposalDto).GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain("StudentId", propertyNames);
        Assert.DoesNotContain("StudentName", propertyNames);
        Assert.DoesNotContain("StudentEmail", propertyNames);
    }

    [Fact]
    public async Task BlindMatching_ShouldReturnAllowedCoreFields_TitleAndCategoryEquivalent()
    {
        // This test checks the core allowed fields (Title and ResearchAreaName as category equivalent).
        using var context = TestHelpers.CreateContext();

        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "Data Science", IsActive = true });
        context.SupervisorExpertises.Add(new SupervisorExpertise
        {
            SupervisorId = "supervisor-1",
            ResearchAreaId = 1
        });
        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Data Science Blind Project",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new ProposalService(
            context,
            TestHelpers.CreateUserManagerMock().Object,
            new Mock<IAuditService>().Object);

        var result = (await service.GetAnonymousProposalsForSupervisorAsync("supervisor-1")).ToList();

        Assert.Single(result);
        Assert.Equal("Data Science Blind Project", result[0].Title);
        Assert.Equal("Data Science", result[0].ResearchAreaName);
    }

    [Fact]
    public async Task BlindMatching_ShouldRevealIdentityOnlyAfterConfirmation()
    {
        // This test verifies reveal flag changes only when the match is confirmed.
        using var context = TestHelpers.CreateContext();

        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Proposal For Reveal Test",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);

        var interest = await service.ExpressInterestAsync("supervisor-1", 1);
        Assert.False(interest.IsRevealed);

        var confirmed = await service.ConfirmMatchAsync("supervisor-1", 1);
        Assert.True(confirmed.IsRevealed);
    }
}
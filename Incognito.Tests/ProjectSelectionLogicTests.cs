using IncognitoPAS.Models;
using IncognitoPAS.Services;
using Moq;

namespace Incognito.Tests;

public class ProjectSelectionLogicTests
{
    [Fact]
    public async Task ProjectSelection_ShouldSetStatusToMatched_WhenSupervisorConfirms()
    {
        // This test verifies project selection by supervisor updates proposal status to Matched.
        using var context = TestHelpers.CreateContext();

        var proposal = new ProjectProposal
        {
            Id = 1,
            Title = "Selectable Project Title",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        };
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);

        await service.ExpressInterestAsync("supervisor-1", 1);
        var confirmed = await service.ConfirmMatchAsync("supervisor-1", 1);

        var saved = await context.Proposals.FindAsync(1);
        Assert.NotNull(saved);
        Assert.True(confirmed.IsConfirmed);
        Assert.Equal(ProposalStatus.Matched, saved!.Status);
    }

    [Fact]
    public async Task ProjectSelection_ShouldRejectConfirmation_WhenNoInterestExists()
    {
        // This test ensures a supervisor cannot confirm without selecting first.
        using var context = TestHelpers.CreateContext();
        var service = new MatchingService(context, new Mock<IAuditService>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConfirmMatchAsync("supervisor-1", 100));
    }

    [Fact]
    public async Task ProjectSelection_ShouldRejectDuplicateSelection_BySameSupervisor()
    {
        // This test checks duplicate selection is blocked for the same supervisor and proposal.
        using var context = TestHelpers.CreateContext();

        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Duplicate Interest Check Title",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);

        await service.ExpressInterestAsync("supervisor-1", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExpressInterestAsync("supervisor-1", 1));
    }
}
using IncognitoPAS.Models;
using IncognitoPAS.Services;
using Moq;

namespace Incognito.Tests;

// INTEGRATION TESTS: verifies persisted proposal status transition rules using EF Core InMemory.
public class StatusManagementTests
{
    [Fact]
    public async Task StatusManagement_ShouldMovePendingToUnderReview_OnExpressInterest()
    {
        // This test validates the expected status transition: Pending -> UnderReview.
        using var context = TestHelpers.CreateContext();

        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Status Transition Proposal",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);
        await service.ExpressInterestAsync("supervisor-1", 1);

        var saved = await context.Proposals.FindAsync(1);
        Assert.NotNull(saved);
        Assert.Equal(ProposalStatus.UnderReview, saved!.Status);
    }

    [Fact]
    public async Task StatusManagement_ShouldMoveUnderReviewToMatched_OnConfirm()
    {
        // This test validates the expected status transition: UnderReview -> Matched.
        using var context = TestHelpers.CreateContext();

        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Second Status Transition",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.UnderReview
        });
        context.SupervisorMatches.Add(new SupervisorMatch
        {
            ProposalId = 1,
            SupervisorId = "supervisor-1",
            IsConfirmed = false,
            IsRevealed = false
        });
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);
        await service.ConfirmMatchAsync("supervisor-1", 1);

        var saved = await context.Proposals.FindAsync(1);
        Assert.NotNull(saved);
        Assert.Equal(ProposalStatus.Matched, saved!.Status);
    }

    [Fact]
    public async Task StatusManagement_ShouldPreventInvalidTransition_FromMatchedToInterest()
    {
        // This test ensures invalid transition is blocked when status is already Matched.
        using var context = TestHelpers.CreateContext();

        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Matched Proposal Block Test",
            Abstract = new string('A', 120),
            StudentId = "student-1",
            ResearchAreaId = 1,
            Status = ProposalStatus.Matched
        });
        await context.SaveChangesAsync();

        var service = new MatchingService(context, new Mock<IAuditService>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExpressInterestAsync("supervisor-1", 1));
    }
}
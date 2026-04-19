using IncognitoPAS.Models;
using IncognitoPAS.Services;
using Moq;

namespace Incognito.Tests;

public class MatchingLogicTests
{
    [Fact]
    public async Task MatchStudentSkillsWithSupervisorExpertise_ShouldReturnOnlyRelevantResearchArea()
    {
        // This test checks the expertise-based matching filter for supervisors.
        using var context = TestHelpers.CreateContext();

        var aiArea = new ResearchArea { Id = 1, Name = "AI", IsActive = true };
        var secArea = new ResearchArea { Id = 2, Name = "Security", IsActive = true };
        context.ResearchAreas.AddRange(aiArea, secArea);

        context.SupervisorExpertises.Add(new SupervisorExpertise
        {
            SupervisorId = "supervisor-1",
            ResearchAreaId = 1
        });

        context.Proposals.AddRange(
            new ProjectProposal
            {
                Id = 1,
                Title = "AI-based Topic for Matching",
                Abstract = new string('A', 120),
                StudentId = "student-1",
                ResearchAreaId = 1,
                Status = ProposalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new ProjectProposal
            {
                Id = 2,
                Title = "Security-based Topic",
                Abstract = new string('B', 120),
                StudentId = "student-2",
                ResearchAreaId = 2,
                Status = ProposalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var service = new ProposalService(
            context,
            TestHelpers.CreateUserManagerMock().Object,
            new Mock<IAuditService>().Object);

        var result = (await service.GetAnonymousProposalsForSupervisorAsync("supervisor-1")).ToList();

        Assert.Single(result);
        Assert.Equal("AI-based Topic for Matching", result[0].Title);
    }

    [Fact]
    public async Task MatchStudentSkillsWithSupervisorExpertise_ShouldReturnEmpty_WhenNoExpertiseFound()
    {
        // This test covers the edge case where supervisor has no expertise tags.
        using var context = TestHelpers.CreateContext();

        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
        context.Proposals.Add(new ProjectProposal
        {
            Id = 1,
            Title = "Another AI Proposal Title",
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

        var result = (await service.GetAnonymousProposalsForSupervisorAsync("supervisor-without-expertise")).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task MatchStudentSkillsWithSupervisorExpertise_ShouldExcludeMatchedStatus()
    {
        // This test ensures only Pending and UnderReview items are considered for matching.
        using var context = TestHelpers.CreateContext();

        context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
        context.SupervisorExpertises.Add(new SupervisorExpertise
        {
            SupervisorId = "supervisor-1",
            ResearchAreaId = 1
        });
        context.Proposals.AddRange(
            new ProjectProposal
            {
                Id = 1,
                Title = "Pending Proposal for AI",
                Abstract = new string('A', 120),
                StudentId = "student-1",
                ResearchAreaId = 1,
                Status = ProposalStatus.Pending
            },
            new ProjectProposal
            {
                Id = 2,
                Title = "Matched Proposal Not Visible",
                Abstract = new string('B', 120),
                StudentId = "student-2",
                ResearchAreaId = 1,
                Status = ProposalStatus.Matched
            });
        await context.SaveChangesAsync();

        var service = new ProposalService(
            context,
            TestHelpers.CreateUserManagerMock().Object,
            new Mock<IAuditService>().Object);

        var result = (await service.GetAnonymousProposalsForSupervisorAsync("supervisor-1")).ToList();

        Assert.Single(result);
        Assert.Equal("Pending", result[0].Status);
    }
}
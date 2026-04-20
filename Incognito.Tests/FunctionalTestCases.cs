namespace Incognito.Tests;

/// <summary>
/// FUNCTIONAL TESTS: documented manual user journeys and security checks.
/// These cases are evidence of manual validation and are executed in the running application.
/// </summary>
public static class FunctionalTestCases
{
    public static IReadOnlyList<ManualFunctionalCase> Cases { get; } =
        new List<ManualFunctionalCase>
        {
            new(
                "TC-F01",
                "Login with invalid password is rejected",
                "Navigate to /Account/Login, enter valid email and wrong password, then submit.",
                "Error is shown and user remains on login page.",
                "Manual/Functional"),
            new(
                "TC-F02",
                "Student submission without abstract is blocked",
                "Login as Student, open create proposal, leave abstract empty, submit.",
                "Validation error for abstract length is displayed.",
                "Manual/Functional"),
            new(
                "TC-F03",
                "Supervisor blind browse hides student identity",
                "Login as Supervisor and open blind proposal browsing.",
                "Only proposal content and research-fit fields are shown; no student identity fields.",
                "Manual/Functional"),
            new(
                "TC-F04",
                "Confirmed match reveals identities",
                "Supervisor expresses interest and confirms a match.",
                "Supervisor and student views both show revealed counterpart identity after confirmation.",
                "Manual/Integration"),
            new(
                "TC-F05",
                "Matched proposal cannot be edited",
                "Student with matched proposal navigates to edit route.",
                "User is blocked and receives pending-only edit restriction message.",
                "Manual/Functional"),
            new(
                "TC-F06",
                "Unauthenticated access redirects to login",
                "Sign out and navigate directly to a protected student route.",
                "Request is redirected to login with return URL.",
                "Manual/Security"),
            new(
                "TC-F07",
                "Role guard blocks unauthorized supervisor route access",
                "Login as Student and navigate to supervisor-only browse route.",
                "Access denied behavior is triggered.",
                "Manual/Security"),
            new(
                "TC-F08",
                "New research area appears in dependent flows",
                "Create research area as ModuleLeader and revisit student/supervisor forms.",
                "New area appears in proposal and expertise selection.",
                "Manual/Functional"),
            new(
                "TC-F09",
                "Admin-created user can sign in",
                "Create a new student user from admin panel and attempt login.",
                "New user credentials are accepted.",
                "Manual/Functional"),
            new(
                "TC-F10",
                "Blind proposal API payload never leaks identity",
                "Inspect supervisor browse network responses.",
                "Response payload excludes StudentId, StudentName, and student email fields.",
                "Manual/Security")
        };
}

public sealed record ManualFunctionalCase(
    string Id,
    string Scenario,
    string Steps,
    string ExpectedResult,
    string Category);
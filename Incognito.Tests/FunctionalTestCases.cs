namespace Incognito.Tests;

/// <summary>
/// Functional test cases documented as comments.
/// These are designed for manual testing and documentation purposes.
/// </summary>
public class FunctionalTestCases
{
    // TC-F01: Login with invalid password → expect validation error on login page
    // Steps: Navigate to /Account/Login, enter valid email, enter wrong password, click Login
    // Expected: Error message displayed, user remains on login page
    // Test Type: Manual/Functional

    // TC-F02: Student submits proposal without abstract → form rejects submission
    // Steps: Login as Student, navigate to /Student/Create, fill Title and TechStack only, click Submit
    // Expected: Validation error "Abstract must be between 100 and 1000 characters"
    // Test Type: Manual/Functional

    // TC-F03: Supervisor browses proposals → no student names visible anywhere
    // Steps: Login as Supervisor, navigate to /Supervisor/Browse
    // Expected: Table shows Title, Abstract, TechStack, Research Area - NO student names or IDs
    // Test Type: Manual/Functional

    // TC-F04: Supervisor confirms match → both parties see identity reveal card
    // Steps: Supervisor browses, expresses interest, confirms match
    // Expected: Supervisor sees confirmation with revealed student details. Student sees RevealCard partial.
    // Test Type: Manual/Integration

    // TC-F05: Student tries to edit Matched proposal → redirect with error message
    // Steps: Student has matched proposal, tries to navigate to /Student/Edit/{id}
    // Expected: Redirect to Dashboard with error TempData["Error"] = "Only pending proposals can be edited."
    // Test Type: Manual/Functional

    // TC-F06: Unauthenticated user accesses /Student/Dashboard → redirect to login
    // Steps: Close all sessions, navigate directly to /Student/Dashboard
    // Expected: 302 redirect to /Account/Login?ReturnUrl=/Student/Dashboard
    // Test Type: Manual/Security

    // TC-F07: Student role accesses /Supervisor/Browse → 403 Access Denied page
    // Steps: Login as Student, try to navigate to /Supervisor/Browse
    // Expected: 403 error page or redirect to appropriate error page
    // Test Type: Manual/Security

    // TC-F08: ModuleLeader creates research area → appears in dropdowns
    // Steps: Login as ModuleLeader, navigate to /ModuleLeader/CreateResearchArea, create "Blockchain"
    // Expected: New area appears in Student/Create dropdown and Supervisor/SetExpertise
    // Test Type: Manual/Functional

    // TC-F09: SysAdmin creates new user → user can login immediately
    // Steps: Login as SysAdmin, navigate to /Admin/CreateUser, create new Student
    // Expected: New user can login with provided credentials
    // Test Type: Manual/Functional

    // TC-F10: BlindProposalDto never leaks identity → security validation
    // Steps: Inspect network responses when Supervisor browses proposals
    // Expected: No StudentId, StudentName, or any user identity field in JSON responses
    // Test Type: Security/Manual
}
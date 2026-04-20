# Testing Strategy

## Test Pyramid and Scope
This project uses a mixed strategy aligned to the coursework rubric:

- Unit tests: isolate business/service logic with Moq where practical.
- Integration tests: verify persistence and state transitions with EF Core InMemory.
- Functional tests: document manual user journeys and security/access flows.

The suite does not claim full test-first coverage from initial project inception.
Selected business-rule refinements were validated in a test-first manner during late-stage hardening.

## Why Unit vs Integration vs Functional Are Separated
- Unit tests are used when dependency isolation is important (for example authentication and service dependency interactions).
- Integration tests are used when behavior depends on EF queries, entity state changes, and workflow persistence.
- Functional tests are maintained as documented manual scenarios for end-to-end user journey evidence.

## Current Test Classification
### Unit Tests (logic isolation with Moq)
- AuthenticationLogicTests.cs

### Service-level tests with Moq + EF InMemory (hybrid, honest classification)
- ProposalServiceTests.cs
- MatchingServiceTests.cs

### Integration Tests (database-backed behavior)
- BlindMatchingLogicTests.cs
- MatchingLogicTests.cs
- ProjectSelectionLogicTests.cs
- ProjectSubmissionValidationTests.cs
- ProposalIntegrationTests.cs
- StatusManagementTests.cs

### Functional Tests (manual documented journeys)
- FunctionalTestCases.cs

## Where Moq Is Used and Why
Moq is used to isolate service dependencies and to verify interaction contracts:

- Authentication tests:
  - UserManager.FindByEmailAsync call verification
  - SignInManager.CheckPasswordSignInAsync call verification
  - Positive/negative role lookup verification
- Proposal tests:
  - User lookup and role lookup verification
  - Audit logging verification for withdraw workflow
  - Negative verification when validation fails early
- Matching tests:
  - Interest/confirm/reveal audit event verification
  - Negative verification when duplicate interest or invalid transitions fail
- Helper factories:
  - UserManager and SignInManager mocks are centralized in TestHelpers.cs

## Blind-Match Rule Coverage
The suite verifies these core blind-matching rules:

- Supervisor browse does not expose student identity fields.
- Expressed interest moves status from Pending to UnderReview.
- Confirmed match moves status to Matched.
- Identity reveal occurs only after confirmation.
- Students cannot read other students' proposal lists.
- Matched proposals cannot be withdrawn.

## TDD Evidence (Honest Scope)
The repository does not claim complete historical TDD for all features.
For selected late-stage refinements, test-first checks were used to lock behavior expectations.

| Scenario | Test-first evidence | Fix/implementation | Outcome |
|----------|---------------------|--------------------|---------|
| Duplicate supervisor interest blocked | Added/asserted exception-path behavior before final verification updates | No production code change required; service already enforced rule | Rule is now explicitly guarded in tests |
| Matched proposal cannot be withdrawn | Added negative-path assertion and no-audit verification | No production code change required; existing guard confirmed | Regression protection added for matched-state withdrawal |
| Identity reveal only after confirmation | Strengthened confirmation assertions including reveal flag and audit events | No production code change required; workflow already implemented | Reveal behavior is now clearly evidenced |
| Invalid login rejected | Added interaction-level verification for unknown email and password checks | No production code change required; auth logic already correct | Authentication failure paths now have explicit mock verification |

## Coverage and Execution
Prerequisite packages are already referenced in Incognito.Tests.csproj:

- Moq
- FluentAssertions
- coverlet.collector

Run tests:

```bash
dotnet test Incognito.Tests/Incognito.Tests.csproj
```

Collect cross-platform coverage:

```bash
dotnet test Incognito.Tests/Incognito.Tests.csproj --collect:"XPlat Code Coverage"
```

Generate HTML report (if ReportGenerator is installed):

```bash
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### Sample local evidence (2026-04-20)
- `dotnet test -c Release`: 44 passed, 0 failed.
- `dotnet test -c Release --collect:"XPlat Code Coverage"` generated Cobertura output.
- Coverage snapshot from generated report:
  - Overall project line-rate: 11.09% (includes full application startup and untested layers).
  - AuthService line-rate: 100%.
  - MatchingService line-rate: 100%.
  - ProposalService line-rate: 97.36%.

This demonstrates strong service-layer coverage while making no claim of full-application 80% coverage.

## Suggested Commit Messages
- test: add verify assertions for auth service mocking
- test: classify EF-backed tests as integration tests
- test: add audit logging verification for proposal and matching services
- docs: add testing strategy and TDD evidence
- chore: improve coverage reporting instructions

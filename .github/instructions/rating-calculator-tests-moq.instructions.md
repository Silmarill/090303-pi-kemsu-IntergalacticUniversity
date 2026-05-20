---
applyTo: "**/*.cs"
---

# Instruction Identity

Instruction source: `.github/instructions/rating-calculator-tests-moq.instructions.md`
Applies to: `**/*.cs`
Purpose: Required advanced Moq scenarios and test organization for the `RatingCalculator` assignment.

When generating Pull request overview, include this instruction source for inspected `.cs` files in `IntergalacticUniversity.Tests` or tests related to `RatingCalculator`.

# Block 3: Advanced Moq Scenarios

Check that the test suite contains these four scenarios:

1. Repository call verification: after `CalculateCurrentScore(student, course)`, verify both repositories were called exactly once with the same `student` and `course` objects:
   - `mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);`
   - `mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);`
2. `null` handling:
   - `GetRawScore` returns `null`, attendance exists -> result contains only attendance points.
   - `GetAttendedClasses` returns `null`, assignments exist -> result contains only assignment points.
   - Calculator must not throw just because a repository returned `null`.
3. `CalculateTotalScore` must not trigger repeated repository calls. One call to `CalculateTotalScore` should lead to exactly one call to each repository method. Do not call `CalculateCurrentScore` separately in the same test before verification, because that ruins the call count.
4. Exception propagation: configure a repository mock with `Throws<TimeoutException>()` and verify `Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(student, course));`. The calculator should not swallow infrastructure exceptions.

# Moq Usage Rules

- Use `new Mock<T>()` for simple scenarios.
- Prefer `new Mock<T>(MockBehavior.Strict)` for interaction and exception tests.
- Use `Setup(...).Returns(...)` for deterministic input.
- Use `Verify(..., Times.Once)` to check interactions.
- Use `It.IsAny<T>()` only when concrete arguments are irrelevant.
- If the assignment asks to verify exact `student` and `course`, do not replace them with `It.IsAny<Student>()` and `It.IsAny<Course>()`.

# Suggested Test Project Structure

Preferred structure:

```text
IntergalacticUniversity.Tests
├── SimpleTests
│   └── MinimumMaximumScenarios.cs
├── ParameterizedTests
│   ├── GradeConversionTests.cs
│   ├── AssignmentsPortionTests.cs
│   └── CombinedScenariosTests.cs
├── MockInteractionTests
│   ├── RepositoryCallVerificationTests.cs
│   ├── NullHandlingTests.cs
│   └── ExceptionPropagationTests.cs
└── Common
    └── TestDataFactory.cs
```

Alternative structure is acceptable if all 12 required checks are present and easy to find.

# Test Anti-Patterns

Flag these when they appear:

- Real repositories instead of Moq.
- Tests that depend on console, files, network, time, or execution order.
- `Assert.NotNull(result)` without checking the actual expected value.
- Duplicated tests with no new scenario.
- Complex loops or conditions inside tests.
- Expected value computed by the method under test.
- `RatingCalculator` tests placed in `Console` or `Core`.
- Test project referencing console app instead of `Core`.
- `RatingCalculator` swallowing repository exceptions.
- Missing boundary tests for `ConvertToGrade`.

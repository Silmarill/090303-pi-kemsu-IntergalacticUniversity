---
applyTo: "**/*.cs"
---

# Instruction Identity

Instruction source: `.github/instructions/rating-calculator-tests-basic.instructions.md`
Applies to: `**/*.cs`
Purpose: Required NUnit/Moq test coverage for blocks 1 and 2 of the `RatingCalculator` assignment.

When generating Pull request overview, include this instruction source for inspected `.cs` files in `IntergalacticUniversity.Tests` or tests related to `RatingCalculator`.

# General Test Rules

- Put `RatingCalculator` tests in `IntergalacticUniversity.Tests` only.
- Do not test `Program.cs` or dummy repositories from `IntergalacticUniversity.Console`.
- Test project must reference `IntergalacticUniversity.Core`, not the console `.exe`.
- Use NUnit: `[TestFixture]`, `[Test]`, `[TestCase]`, `[SetUp]`, `[TearDown]` where appropriate.
- Use Moq for `IAttendanceRepository` and `IAssignmentsRepository`.
- Do not use real database, files, network, API, console input/output, `Thread.Sleep`, or nondeterministic dependencies.
- Calculate expected values manually; never call the tested method to compute expected values.
- For `double`, use tolerance: `Assert.That(actual, Is.EqualTo(expected).Within(0.001));`.
- Tests must be fast, independent, and clearly named: `MethodName_WhenCondition_ReturnsExpectedResult`.

# Block 1: Ordinary `[Test]` Scenarios

Check that the test suite contains these four non-parameterized scenarios:

1. Minimum values: course type `Exam`; both repositories return `null`; `CalculateCurrentScore` returns `0`.
2. Maximum values: `Exam`, `MaxRawAssignmentsScore = 800`, `TotalClasses = 40`, `MaxAttendanceScore = 20`, raw `800`, attended `40`; current score is `60`; also verify `ConvertToGrade(100)` returns `"Отлично"`.
3. Upper cap: `Credit`, `MaxAttendanceScore = 15`, raw score above max, full attendance; current score must not exceed `80`.
4. Credit total: current score is `75` of `80`, credit score is `20`; `CalculateTotalScore` returns `95`; if the sum exceeds `100`, it must be capped.

# Block 2: Parameterized `[TestCase]` Scenarios

Check that the test suite contains these four parameterized groups:

1. `ConvertToGrade` boundary values:
   - `49` -> `"Неудовлетворительно"`
   - `51`, `60` -> `"Удовлетворительно"`
   - `66`, `75` -> `"Хорошо"`
   - `86`, `100` -> `"Отлично"`
2. Assignment score scaling: `Exam`, `MaxRawAssignmentsScore = 1000`, `MaxAttendanceScore = 20`, full attendance; test at least raw `0%`, `30%`, `100%` and include the fixed attendance points in expected current score.
3. Attendance scaling: `Credit`, `MaxAttendanceScore = 10`, assignments at `100%`; test attendance `100%`, `50%`, `0%` and expect `70 + attendancePart`.
4. Combined cases: `Exam`, `MaxRawAssignmentsScore = 600`, `TotalClasses = 20`, `MaxAttendanceScore = 15`; use 3-4 combinations of `rawPercent`, `attendancePercent`, `expectedCurrent` across low/mid/high classes.

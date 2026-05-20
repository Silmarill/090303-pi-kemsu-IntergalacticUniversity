---
applyTo: "**/*.cs"
---

# Instruction Identity

Instruction source: `.github/instructions/rating-calculator-context.instructions.md`
Applies to: `**/*.cs`
Purpose: Domain context and business rules for the IntergalacticUniversity `RatingCalculator` assignment.

When generating Pull request overview, include this instruction source for inspected `.cs` files related to `IntergalacticUniversity.Core`, `IntergalacticUniversity.Tests`, or `RatingCalculator`.

# Project Context

The solution is expected to contain three projects:

- `IntergalacticUniversity.Core`: models, interfaces, and business logic.
- `IntergalacticUniversity.Console`: demo console app and dummy repositories only.
- `IntergalacticUniversity.Tests`: NUnit tests using Moq.

Main production type: `IntergalacticUniversity.Core.Services.RatingCalculator`.

Related contracts:

- `Student`: `Id`, `Name`.
- `Course`: `CourseId`, `Name`, `Type`, `MaxRawAssignmentsScore`, `TotalClasses`, `MaxAttendanceScore`.
- `ExamType`: `Exam`, `Credit`.
- `IAttendanceRepository.GetAttendedClasses(Student student, Course course)` returns `int?`.
- `IAssignmentsRepository.GetRawScore(Student student, Course course)` returns `double?`.

# RatingCalculator Business Rules

Use these rules when reviewing or generating C# code and tests:

- For `Exam`, current score is scaled to `60` points.
- For `Credit`, current score is scaled to `80` points.
- `MaxAttendanceScore` is subtracted from the current-score scale; the remainder is the maximum assignments score.
- `maxAssignments = maxCurrent - course.MaxAttendanceScore`.
- Raw assignment score is normalized by `course.MaxRawAssignmentsScore`.
- Attendance is normalized by `course.TotalClasses`.
- `null` from a repository means missing data and contributes `0` points for that part.
- Assignment points must not exceed `maxAssignments`.
- Attendance points must not exceed `course.MaxAttendanceScore`.
- `CalculateCurrentScore` must not exceed `60` for exam and `80` for credit.
- `CalculateTotalScore` adds exam/credit score and caps the final score at `100`.
- Exam score cap is `40`; credit score cap is `20`.
- `ConvertToGrade(double totalScore)` must map required boundary values correctly:
  - `49` -> `"Неудовлетворительно"`;
  - `51`, `60` -> `"Удовлетворительно"`;
  - `66`, `75` -> `"Хорошо"`;
  - `86`, `100` -> `"Отлично"`.

# Production Code Expectations

- `RatingCalculator` must receive repositories through constructor injection.
- Do not instantiate concrete repositories inside `RatingCalculator`.
- Do not access database, files, network, API, console, or UI from business logic.
- Calculation methods should return values and avoid hidden side effects.
- Do not change public contracts of models, interfaces, or calculator methods without a clear reason.

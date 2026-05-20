---
applyTo: "**/*.cs"
---

# Instruction Identity

Instruction source: `.github/instructions/csharp-lain-core.instructions.md`
Applies to: `**/*.cs`
Purpose: Core C# architectural and beginner-level code quality rules LAIN001-LAIN015.

When generating Pull request overview, include this instruction source for every inspected `.cs` file that matches `**/*.cs`.

# Core LAIN Rules

Review these rules only when you are highly confident that the changed C# code violates them.

- [LAIN001] One class per file. Exceptions: multiple logically related enums or small structs may be grouped only in files named `*Enums.cs` or `*Types.cs`; the student must be ready to explain the grouping.
- [LAIN002] File name must match the public class name exactly, including case. Example: public class `Player` belongs in `Player.cs`.
- [LAIN003] Structure files by folders. Not mandatory, but recommend folders when many models, services, repositories, or tests are mixed together.
- [LAIN004] Avoid code duplication. If a code fragment is copied more than once, suggest extracting a method or test helper.
- [LAIN005] Behavior belongs to the class that owns the state. If a method changes `Player.Health`, ask whether the behavior belongs inside `Player` rather than another object.
- [LAIN006] Separate business logic from UI. Business logic must not call `Console.WriteLine`, `Console.ReadLine`, file I/O, network, database, or UI code. Keep I/O in `Program.cs` or dedicated adapters.
- [LAIN007] Interface names must indicate capability: either end with `-able` (`IDamageable`) or start with `ICan` (`ICanShoot`).
- [LAIN008] File and folder names must not contain spaces or special characters except `_` and `-`.
- [LAIN009] Use `this` only to disambiguate between a parameter and a field, usually in a constructor. In other cases it is redundant.
- [LAIN011] Put each field on a separate line. Multiple fields on one line, such as `int a, b;`, are forbidden.
- [LAIN012] Properties require justification. For simple data storage, prefer fields in this course; if an auto-property appears, ask why a property is needed.
- [LAIN013] Expression-bodied members using `=>` require explanation. If used, ask what it compiles to and why it improves this code.
- [LAIN014] Nullable types such as `int?` require explanation. If `?` appears, ask why missing data must be represented here.
- [LAIN015] Avoid magic numbers. Use named variables or constructor parameters, but do not create silly constants like `int zero = 0`.

# Commenting Style

Group findings by LAIN code after the mandatory PR overview. Each group must explain why the rule matters and point to concrete file/line locations. Keep comments short and educational.

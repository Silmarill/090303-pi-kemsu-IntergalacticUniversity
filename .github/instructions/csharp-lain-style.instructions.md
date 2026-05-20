---
applyTo: "**/*.cs"
---

# Instruction Identity

Instruction source: `.github/instructions/csharp-lain-style.instructions.md`
Applies to: `**/*.cs`
Purpose: C# style, naming, formatting, and maintainability rules LAIN017-LAIN032.

When generating Pull request overview, include this instruction source for every inspected `.cs` file that matches `**/*.cs`.

# Style and Maintainability LAIN Rules

Review these rules only when you are highly confident that the changed C# code violates them.

- [LAIN017] `internal` is not needed in this course. Use `public`, `protected`, or `private`; if another modifier appears, ask for an explanation.
- [LAIN018] Use suffixes for loop and collection variables. Example: `foreach (var factoryItem in factoryList)` instead of `foreach (var factory in factories)`.
- [LAIN019] Use prefix increment `++counter` instead of postfix `counter++` when the old value is not used in the same expression.
- [LAIN020] Indentation is 2 spaces per level. Do not use tabs.
- [LAIN021] Naming conventions: private fields use `_camelCase`; local variables and parameters use `camelCase`; classes, methods, and public members use `PascalCase`.
- [LAIN022] Opening brace must be on the same line, preceded by one space: `if (...) {`. Do not put the opening brace on a new line.
- [LAIN023] Do not use single-letter variable names, even in loops. Use meaningful names such as `planetIndex`, `studentItem`, or `courseCounter`.
- [LAIN024] `readonly` and `const` are allowed only with explanation. Ask the student to explain the difference and why this specific member should be immutable.
- [LAIN025] Use block-scoped namespace: `namespace Name { ... }`. Do not use file-scoped namespace: `namespace Name;`.
- [LAIN027] One method should do one task. If a method is longer than about 20-30 lines without a strong reason, suggest extraction.
- [LAIN028] Do not use `static` for mutable state. Static counters may be acceptable; player data or changing domain state should not be static.
- [LAIN029] Use `var` only when the type is obvious from `new TypeName(...)`. For literals like `var count = 5`, use the explicit type.
- [LAIN032] All elements inside the same block must have the same indentation level.

# Review Behavior

Pick the most important issue if one line violates several rules. Too many comments overwhelm beginners, and yes, the student already has enough dragons to fight.

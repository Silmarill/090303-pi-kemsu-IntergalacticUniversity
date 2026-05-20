---
applyTo: "**/*.cs"
---

# GitHub Copilot Instructions для C#-файлов проекта IntergalacticUniversity

Эти инструкции применяются ко всем `*.cs` файлам репозитория. При генерации, исправлении или ревью C#-кода ориентируйся на учебное задание: нужно написать и поддерживать юнит-тесты для `RatingCalculator` с использованием NUnit и Moq.

## Контекст проекта

Решение состоит из трёх проектов:

- `IntergalacticUniversity.Core` — бизнес-логика, модели, интерфейсы и `RatingCalculator`.
- `IntergalacticUniversity.Console` — демонстрационная консольная программа и dummy-репозитории.
- `IntergalacticUniversity.Tests` — юнит-тесты на NUnit с моками Moq.

Главная проверяемая логика находится в `IntergalacticUniversity.Core.Services.RatingCalculator`.

Связанные типы:

- `Student`: `Id`, `Name`.
- `Course`: `CourseId`, `Name`, `Type`, `MaxRawAssignmentsScore`, `TotalClasses`, `MaxAttendanceScore`.
- `ExamType`: `Exam`, `Credit`.
- `IAttendanceRepository.GetAttendedClasses(Student student, Course course)` возвращает `int?`.
- `IAssignmentsRepository.GetRawScore(Student student, Course course)` возвращает `double?`.

## Ожидаемая бизнес-логика RatingCalculator

При проверке или генерации тестов учитывай следующие правила расчёта:

- Для `Exam` текущая успеваемость приводится к шкале `60` баллов.
- Для `Credit` текущая успеваемость приводится к шкале `80` баллов.
- `MaxAttendanceScore` отнимается от текущей шкалы, остаток — максимум за задания:
  - `maxAssignments = maxCurrent - course.MaxAttendanceScore`.
- Сырые баллы за задания нормализуются относительно `course.MaxRawAssignmentsScore`.
- Посещаемость нормализуется относительно `course.TotalClasses`.
- `null` от репозитория означает отсутствие данных и должен давать `0` баллов по соответствующей части.
- Баллы за задания не должны превышать `maxAssignments`.
- Баллы за посещаемость не должны превышать `course.MaxAttendanceScore`.
- `CalculateCurrentScore` не должен возвращать больше `60` для экзамена и больше `80` для зачёта.
- `CalculateTotalScore` добавляет балл экзамена или зачёта:
  - для `Exam` максимум промежуточной аттестации — `40`;
  - для `Credit` максимум промежуточной аттестации — `20`;
  - итоговый балл не должен превышать `100`.
- `ConvertToGrade(double totalScore)` переводит итоговый балл в строковую оценку. Обязательно проверяй значения из задания:
  - `49` → `"Неудовлетворительно"`;
  - `51`, `60` → `"Удовлетворительно"`;
  - `66`, `75` → `"Хорошо"`;
  - `86`, `100` → `"Отлично"`.

## Общие правила для тестов

- Все тесты для `RatingCalculator` размещай только в проекте `IntergalacticUniversity.Tests`.
- Не тестируй `Program.cs` и dummy-репозитории из `IntergalacticUniversity.Console`.
- Тестовый проект должен ссылаться на `IntergalacticUniversity.Core`, а не на консольный `.exe`.
- Используй NUnit:
  - `[TestFixture]` для тестовых классов;
  - `[Test]` для обычных тестов;
  - `[TestCase]` для параметризованных тестов;
  - `[SetUp]` и `[TearDown]` там, где есть повторяющаяся инициализация.
- Используй Moq для `IAttendanceRepository` и `IAssignmentsRepository`.
- Не используй реальные БД, файлы, сеть, API, `Thread.Sleep`, консольный ввод/вывод или недетерминированные зависимости.
- Ожидаемые значения в тестах считай вручную. Не вызывай проверяемый метод для вычисления expected-значения.
- Для `double` используй допуск:
  - `Assert.That(actual, Is.EqualTo(expected).Within(0.001));`
- Один тест должен проверять один сценарий или одну группу эквивалентных параметризованных сценариев.
- Тесты должны быть автономными, быстрыми и независимыми от порядка запуска.
- Не создавай тесты вида `Assert.NotNull(result)` без проверки конкретного ожидаемого значения.
- Не называй тесты `Test1`, `UnitTest1`, `Method_Test`. Используй понятный шаблон:
  - `MethodName_WhenCondition_ReturnsExpectedResult`.

## Обязательное покрытие: блок 1 — обычные тесты `[Test]`

При проверке тестового проекта убедись, что есть 4 непараметризованных сценария.

### 1.1 Минимальные значения — всё на нуле

- Курс: `Exam`.
- `GetRawScore` возвращает `null`.
- `GetAttendedClasses` возвращает `null`.
- `CalculateCurrentScore` должен вернуть `0`.

### 1.2 Максимальные значения — 100% заданий и 100% посещаемости

- Курс: `Exam`.
- `MaxRawAssignmentsScore = 800`.
- `TotalClasses = 40`.
- `MaxAttendanceScore = 20`.
- `rawScore = 800`.
- `attended = 40`.
- Ожидаемый `current = 60`.
- Дополнительно проверить: `ConvertToGrade(100)` возвращает `"Отлично"`.

### 1.3 Ограничение сверху для текущей успеваемости

- Курс: `Credit`.
- `maxCurrent = 80`.
- `MaxAttendanceScore = 15`.
- `maxAssignments = 65`.
- `rawScore` больше `MaxRawAssignmentsScore`, например `1200` при максимуме `1000`.
- Посещаемость — `100%`.
- `CalculateCurrentScore` не должен вернуть больше `80`.

### 1.4 Итоговый балл с зачётом

- Курс: `Credit`.
- Текущая успеваемость должна получиться `75` из `80`.
- Балл за зачёт — `20`.
- `CalculateTotalScore` должен вернуть `95`.
- При сценариях с превышением итог должен обрезаться до `100`.

## Обязательное покрытие: блок 2 — параметризованные тесты `[TestCase]`

При проверке тестового проекта убедись, что есть 4 группы параметризованных проверок.

### 2.1 Границы перевода баллов в оценку

Используй `[TestCase]` для `ConvertToGrade`:

```csharp
[TestCase(49, "Неудовлетворительно")]
[TestCase(51, "Удовлетворительно")]
[TestCase(60, "Удовлетворительно")]
[TestCase(66, "Хорошо")]
[TestCase(75, "Хорошо")]
[TestCase(86, "Отлично")]
[TestCase(100, "Отлично")]
```

### 2.2 Параметризация баллов за задания

- Метод: `CalculateCurrentScore`.
- Курс: `Exam`.
- `MaxRawAssignmentsScore = 1000`.
- `MaxAttendanceScore = 20`.
- `maxAssignments = 40`.
- Посещаемость фиксированная: `100%`.
- Проверить минимум:
  - `raw = 0%` → assignments part `0`;
  - `raw = 30%` → assignments part `0.3 * 40 = 12`;
  - `raw = 100%` → assignments part `40`.
- Итоговый expected должен учитывать фиксированную посещаемость: `expectedCurrent = expectedAssignmentsPart + 20`.

### 2.3 Параметризация посещаемости

- Метод: `CalculateCurrentScore`.
- Курс: `Credit`.
- `maxCurrent = 80`.
- `MaxAttendanceScore = 10`.
- `maxAssignments = 70`.
- Задания фиксированы: `100%`, то есть `70` баллов.
- Проверить минимум:
  - `100%` посещаемости → attendance part `10`;
  - `50%` посещаемости → attendance part `5`;
  - `0%` посещаемости → attendance part `0`.
- Итоговый expected: `70 + attendancePart`.

### 2.4 Комбинированный параметризованный тест

- Метод: `CalculateCurrentScore`.
- Курс: `Exam`.
- `MaxRawAssignmentsScore = 600`.
- `TotalClasses = 20`.
- `MaxAttendanceScore = 15`.
- `maxAssignments = 45`.
- Тест должен принимать минимум три параметра:
  - `rawPercent`;
  - `attendancePercent`;
  - `expectedCurrent`.
- Выбери 3–4 комбинации из разных классов эквивалентности: низкий, средний, высокий процент заданий и посещаемости.
- Expected считай вручную по формуле:
  - `expectedCurrent = rawPercent * 45 + attendancePercent * 15`.

## Обязательное покрытие: блок 3 — продвинутое использование Moq

При проверке тестового проекта убедись, что есть 4 сценария с моками.

### 3.1 Проверка вызова репозиториев с правильными аргументами

- После вызова `CalculateCurrentScore(student, course)` проверь:

```csharp
mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
```

- Проверка должна использовать именно те объекты `student` и `course`, которые передавались в метод.

### 3.2 Обработка `null` от репозитория

Нужны сценарии, где:

- `GetRawScore` возвращает `null`, а посещаемость задана — результат содержит только баллы за посещаемость.
- `GetAttendedClasses` возвращает `null`, а задания заданы — результат содержит только баллы за задания.
- Метод не должен выбрасывать исключение из-за `null`.

### 3.3 `CalculateTotalScore` не вызывает репозитории повторно

- При одном вызове `CalculateTotalScore` каждый метод репозитория должен быть вызван ровно один раз:

```csharp
mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
```

- Не допускай реализации теста, которая сама вызывает `CalculateCurrentScore` перед `CalculateTotalScore`, потому что это исказит счётчик вызовов.

### 3.4 Проброс исключений от репозитория

- Настрой мок через `Throws<TimeoutException>()`.
- Проверь, что `RatingCalculator` не проглатывает исключение:

```csharp
Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(student, course));
```

- Не добавляй в `RatingCalculator` пустой `catch`, который скрывает инфраструктурные ошибки.

## Рекомендации по структуре тестового проекта

Предпочтительная структура:

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

Допускается другая структура, если все 12 проверок из задания явно реализованы и читаемо названы.

## Правила для production-кода `Core`

Если редактируется код `IntergalacticUniversity.Core`, соблюдай тестируемость:

- `RatingCalculator` должен получать зависимости через конструктор.
- Не создавай внутри `RatingCalculator` конкретные репозитории через `new`.
- Не обращайся напрямую к БД, файлам, сети, API или консоли из бизнес-логики.
- Методы расчёта должны возвращать значения, а не писать результат в `Console.WriteLine`.
- Не добавляй скрытые зависимости и глобальное состояние.
- Не изменяй публичные контракты моделей, интерфейсов и методов без явной необходимости.
- Не смешивай бизнес-логику с UI, вводом/выводом или демонстрационным кодом.

## Правила использования Moq

- Для обычных сценариев можно использовать `new Mock<T>()`.
- Для блока 3 предпочтительно использовать `new Mock<T>(MockBehavior.Strict)`, чтобы тест сразу падал при неожиданном вызове.
- Используй `Setup(...).Returns(...)` для детерминированных входных данных.
- Используй `Verify(..., Times.Once)` для проверки взаимодействий.
- Используй `It.IsAny<T>()` только тогда, когда конкретные аргументы не важны для смысла теста.
- Если по заданию нужно проверить конкретные `student` и `course`, не заменяй их на `It.IsAny<Student>()` и `It.IsAny<Course>()`.
- Для исключений используй `Throws<TimeoutException>()` или другой конкретный тип исключения из сценария.

## Именование

Хорошие имена тестовых классов:

- `RatingCalculatorTests`
- `ConvertToGradeTests`
- `AttendanceAndAssignmentsScenarios`
- `RepositoryCallVerificationTests`
- `ExceptionPropagationTests`

Хорошие имена методов:

- `CalculateCurrentScore_WhenNoData_ReturnsZero`
- `CalculateCurrentScore_WhenExamAndFullMarks_ReturnsSixty`
- `CalculateCurrentScore_WhenCreditScoreExceedsMaximum_CapsAtEighty`
- `CalculateTotalScore_WhenCreditCurrentIsSeventyFiveAndCreditScoreIsTwenty_ReturnsNinetyFive`
- `ConvertToGrade_WhenScoreIsBoundaryValue_ReturnsExpectedGrade`
- `CalculateCurrentScore_WhenRepositoryReturnsNull_UsesZeroForMissingPart`
- `CalculateCurrentScore_WhenRepositoryThrowsTimeoutException_PropagatesException`

Плохие имена:

- `Test1`
- `UnitTest1`
- `Check`
- `CalculatorTest`
- `Method_Test`

## Анти-паттерны, которые нужно исправлять или подсвечивать

- Тест использует настоящие репозитории вместо Moq.
- Тест зависит от консоли, файлов, сети, времени или порядка запуска.
- Тест проверяет только, что результат не `null`.
- Тест дублирует другой тест без нового сценария.
- В тесте есть сложные циклы и условия, из-за которых сам тест становится труднее проверяемого кода.
- Expected-значение вычисляется тем же методом, который тестируется.
- Тесты для `RatingCalculator` размещены в `Console` или `Core` вместо `IntergalacticUniversity.Tests`.
- Тестовый проект ссылается на консольный проект вместо `Core`.
- `RatingCalculator` проглатывает исключения от репозиториев.
- `CalculateTotalScore` делает лишние обращения к репозиториям.
- `ConvertToGrade` не покрыт граничными значениями.

## Финальный чек-лист для C#-файлов

Перед завершением генерации или ревью проверь:

- Есть все 12 сценариев из трёх блоков задания.
- Используются NUnit, Moq, `[Test]`, `[TestCase]`, `[SetUp]`, `[TearDown]` по назначению.
- Есть проверки `null`, граничных значений, ограничения сверху и проброса исключений.
- Есть `Verify(..., Times.Once)` для взаимодействия с репозиториями.
- Тесты не обращаются к реальным источникам данных.
- Тесты читаемые, независимые и быстрые.
- Названия файлов, классов и методов помогают преподавателю быстро понять, какой пункт задания покрыт.

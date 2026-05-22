// SimpleTestExample.cs - Блок 1: Обычные тесты (4 проверки)
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleTestExample {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };

      _examCourse = new Course {
        CourseId = 1,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 1.1: Минимальные значения — всё на нуле
    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult1 = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      // Assert
      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    // Проверка 1.2: Максимальные значения — 100% заданий и 100% посещаемости
    [Test]
    public void CalculateCurrentScore_WhenMaxValues_Returns60AndGradeExcellent() {
      // Arrange
      Course examCourse = new Course {
        CourseId = 1,
        Name = "Экзамен",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 80,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, examCourse)).Returns(80);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult1 = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, examCourse)).Returns(40);

      // Act
      double currentScore = _calculator.CalculateCurrentScore(_student, examCourse);

      // Для ConvertToGrade создаём отдельный калькулятор с моками
      Mock<IAttendanceRepository> mockAttendanceForGrade = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignmentsForGrade = new Mock<IAssignmentsRepository>();
      RatingCalculator calculatorForGrade = new RatingCalculator(mockAttendanceForGrade.Object, mockAssignmentsForGrade.Object);
      string grade = calculatorForGrade.ConvertToGrade(100);

      // Assert
      Assert.That(currentScore, Is.EqualTo(60.0).Within(0.001));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    // Проверка 1.3: Проверка ограничения сверху — сумма не может превысить maxCurrent
    [Test]
    public void CalculateCurrentScore_WhenSumExceedsMaxCurrent_CapsAtMaxCurrent() {
      // Arrange: Credit course (maxCurrent = 80, maxAttendance = 15 -> maxAssignments = 65)
      Course creditCourse = new Course {
        CourseId = 2,
        Name = "Зачёт",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(2000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(30);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, creditCourse);

      // Assert
      Assert.That(result, Is.LessThanOrEqualTo(80.0));
      Assert.That(result, Is.EqualTo(80.0).Within(0.001));
    }

    // Проверка 1.4: Итоговый балл с зачётом — правильное сложение
    [Test]
    public void CalculateTotalScore_ForCreditCourse_WithMaxCredit_Returns95() {
      // Arrange: current score = 75 из 80 возможных
      // Формула: rawScore = (75 - attendancePortion) / maxAssignments * maxRaw
      // attendancePortion = 15 (100% attendance), maxAssignments = 65, maxRaw = 100
      // rawScore = (75 - 15) / 65 * 100 = 60 / 65 * 100 ≈ 92.3077
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(92.3077);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      // Act
      double totalScore = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      // Assert
      Assert.That(totalScore, Is.EqualTo(95.0).Within(0.1));
      Assert.That(totalScore, Is.LessThanOrEqualTo(100.0));
    }
  }
}
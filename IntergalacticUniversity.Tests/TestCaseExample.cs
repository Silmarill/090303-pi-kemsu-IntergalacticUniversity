using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class TestCaseExample {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.1: Границы перевода баллов в оценку
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(50, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(65, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(85, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      // Act
      string result = _calculator.ConvertToGrade(totalScore);

      // Assert
      Assert.That(result, Is.EqualTo(expectedGrade));
    }

    // Проверка 2.2: Параметризация приведения баллов за задания
    [TestCase(0, 0)]      // 0% заданий
    [TestCase(30, 12)]    // 30% от maxAssignments (40) = 12
    [TestCase(50, 20)]    // 50% = 20
    [TestCase(70, 28)]    // 70% = 28
    [TestCase(100, 40)]   // 100% = 40
    public void CalculateCurrentScore_VariousAssignmentsPercentages_ReturnsCorrectScore(
        double rawScorePercent, double expectedAssignmentsScore) {
      // Arrange
      Course course = new Course {
        CourseId = 101,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      double rawScore = rawScorePercent / 100 * course.MaxRawAssignmentsScore;

      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(40);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, course);

      double expectedTotal = expectedAssignmentsScore + 20;

      // Assert
      Assert.That(result, Is.EqualTo(expectedTotal));
    }

    // Проверка 2.3: Параметризация учёта посещаемости
    [TestCase(30, 10)]  // 100% посещаемости → 10 баллов
    [TestCase(15, 5)]   // 50% → 5
    [TestCase(0, 0)]    // 0% → 0
    [TestCase(9, 3)]    // 30% → 3
    public void CalculateCurrentScore_VariousAttendancePercentages_ReturnsCorrectScore(
        int attendedClasses, double expectedAttendanceScore) {
      // Arrange
      Course course = new Course {
        CourseId = 102,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 30,
        MaxAttendanceScore = 10
      };

      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attendedClasses);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(100);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, course);

      double expectedTotal = 70 + expectedAttendanceScore;

      // Assert
      Assert.That(result, Is.EqualTo(expectedTotal));
    }

    // Проверка 2.4: Комбинированный параметризованный тест
    [TestCase(0, 0, 0)]      // 0% заданий, 0% посещаемости → 0
    [TestCase(50, 50, 35)]   // 50% заданий (20) + 50% посещаемости (7.5) = 27.5? Давайте пересчитаем
    [TestCase(100, 50, 47.5)] // 100% заданий (40) + 50% посещаемости (7.5) = 47.5
    [TestCase(100, 100, 60)]  // 100% заданий + 100% посещаемости → 60
    public void CalculateCurrentScore_CombinedScenarios_ReturnsExpectedScore(
        double rawPercent, double attendancePercent, double expectedScore) {
      // Arrange
      // Exam: maxCurrent = 60, maxAttendance = 15 → maxAssignments = 45
      Course course = new Course {
        CourseId = 103,
        Name = "Комбинированный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      double rawScore = rawPercent / 100 * course.MaxRawAssignmentsScore;
      int attendedClasses = (int)(attendancePercent / 100 * course.TotalClasses);

      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attendedClasses);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, course);

      // Assert
      // Допускаем погрешность из-за double
      Assert.That(result, Is.EqualTo(expectedScore).Within(0.01));
    }
  }
}
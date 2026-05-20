using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorVerificationTests {
    // Поля для моков, калькулятора и тестовых данных
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;
    private Student _student;
    private Course _course;

    // Этот метод выполняется перед каждым тестом, обеспечивая чистую инициализацию моков и тестовых данных
    [SetUp]
    public void Setup() {
      // Инициализируем мок-репозитории и калькулятор перед каждым тестом
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      // Создаём тестового студента и курс для использования в тестах
      _student = new Student { Id = 1, Name = "Алексей Звёздный" };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 20
      };
    }

    // Этот тест проверяет, что при вызове метода CalculateCurrentScore калькулятор действительно обращается к репозиториям заданий и посещаемости с правильными параметрами и делает это ровно один раз для каждого репозитория
    [Test]
    public void CalculateCurrentScore_ShouldQueryRepositoriesWithCorrectParametersExactlyOnce() {
      // Arrange
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(80);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(8);

      // Act
      _ = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      // Проверяется, что калькулятор действительно обратился к репозиторию заданий ровно 1 раз
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);

      // Проверяется, что калькулятор действительно обратился к репозиторию посещаемости ровно 1 раз
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }
  }
}
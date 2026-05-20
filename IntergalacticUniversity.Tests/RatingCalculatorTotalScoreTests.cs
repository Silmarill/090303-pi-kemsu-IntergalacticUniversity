using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTotalScoreTests {
    // Поля для моков, калькулятора и тестового студента
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;
    private Student _student;

    // Этот тест проверяет, что при расчёте итогового балла экзамен или зачёт корректно ограничиваются своими максимумами, и что итоговый балл не превышает 100
    [SetUp]
    public void Setup() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
    }

    // Нормальный случай: 60 (текущие) + 25 = 85
    [TestCase(ExamType.Exam, 25.0, 85.0)]

    // Отрицательный экзамен срезается до 0: 60 + 0 = 60
    [TestCase(ExamType.Exam, -5.0, 60.0)]

    // Экзамен выше максимума срезается до 40: 60 + 40 = 100
    [TestCase(ExamType.Exam, 50.0, 100.0)]

    // Зачёт: 80 (текущие) + 15 = 95
    [TestCase(ExamType.Credit, 15.0, 95.0)]

    // Зачёт выше максимума срезается до 20: 80 + 20 = 100
    [TestCase(ExamType.Credit, 30.0, 100.0)]

    public void CalculateTotalScore_BorderValues_ClampsCorrectly(
        ExamType examType, double examOrCreditScore, double expectedTotal) {
      // Arrange
      Course course = new Course {
        Type = examType,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 20
      };

      // Текущие баллы мокаются, чтобы они были 60 для экзамена и 80 для зачёта
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(100);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(10);

      // Act
      double actualTotal = _calculator.CalculateTotalScore(_student, course, examOrCreditScore);

      // Assert
      Assert.That(actualTotal, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}
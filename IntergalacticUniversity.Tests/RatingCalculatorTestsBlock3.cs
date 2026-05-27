using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock3 {
    private const int DefaultAttendance = 10;
    private const int DefaultRawScore = 100;
    private const int FullAttendance = 15;
    private const int TotalScoreAttendance = 10;
    private const int TotalScoreRawScore = 50;

    private RatingCalculator _calculator;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    // SA1202: Public members (тесты) должны быть перед private методами.
    // Сначала идут private поля, потом Helper-метод, потом SetUp, потом Тесты.

    // Helper method to create a real course object with valid data
    private Course CreateExamCourse() {
      return new Course {
        CourseId = 42,
        Name = "Exam Course for Testing",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
    }

    [SetUp]
    public void Setup() {
      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      _student = new Student { Id = 1, Name = "Test Student" };
      _course = CreateExamCourse();
    }

    // Проверка 3.1:
    [Test]
    public void CalculateCurrentScore_WhenCalled_CallsRepositoriesExactlyOnce() {
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(DefaultAttendance);
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns(DefaultRawScore);

      _ = _calculator.CalculateCurrentScore(_student, _course);

      _mockAttendance.Verify(m => m.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(m => m.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.2:
    [Test]
    public void CalculateCurrentScore_WhenAssignmentsNull_CalculatesOnlyAttendance() {
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns((int?)null);
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(FullAttendance);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(10.0).Within(0.001));
    }

    // Проверка 3.3:
    [Test]
    public void CalculateTotalScore_CallsCalculateCurrentScore_DoesNotCallRepositoriesTwice() {
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(TotalScoreAttendance);
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns(TotalScoreRawScore);

      _ = _calculator.CalculateTotalScore(_student, _course);

      _mockAttendance.Verify(m => m.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(m => m.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.4:
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_ThrowsSameException() {
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns(100);

      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Throws<TimeoutException>();

      TimeoutException? ex = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student, _course));

      _mockAssignments.Verify(m => m.GetRawScore(_student, _course), Times.Once);
    }
  }
}
using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class MockAdvancedTests {
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _student = new Student { Id = 67 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments_OnceEach() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(800);

      // Act
      _ = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      _mockAttendance.Verify(repo => repo.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(repo => repo.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_CalculatesOnlyAttendance() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns((double?)null);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      // Посещаемость: 100% от 20 = 20 баллов
      // Задания: 0 баллов
      Assert.That(result, Is.EqualTo(20.0));
    }

    // Обработка null от репозитория — данные о посещаемости отсутствуют
    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_CalculatesOnlyAssignments() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns((int?)null);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(800);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      // Задания: 800/800 = 100% от 40 = 40 баллов
      // Посещаемость: 0 баллов
      Assert.That(result, Is.EqualTo(40.0));
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce_NotTwice() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(800);

      // Act
      _ = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 30);

      // Assert
      _mockAttendance.Verify(repo => repo.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(repo => repo.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_PropagatesException() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course))
          .Throws<TimeoutException>();

      // Act & Assert
      _ = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student, _course));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      // Arrange
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course))
          .Throws<InvalidOperationException>();
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(800);

      // Act & Assert
      _ = Assert.Throws<InvalidOperationException>(() => _calculator.CalculateCurrentScore(_student, _course));
    }
  }
}

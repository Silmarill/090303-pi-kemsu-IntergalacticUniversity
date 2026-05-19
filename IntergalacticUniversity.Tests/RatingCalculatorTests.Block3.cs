using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorMockVerificationTests {
    private const int defaultAttendance = 10;
    private const int defaultRawScore = 100;
    private const int fullAttendance = 15;
    private const int totalScoreAttendance = 10;
    private const int totalScoreRawScore = 50;

    private RatingCalculator _calculator;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    [SetUp]
    public void Setup() {
      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      _student = new Mock<Student>().Object;
      _course = new Mock<Course>().Object;
    }

    // Проверка 3.1:
    [Test]
    public void CalculateCurrentScore_WhenCalled_CallsRepositoriesExactlyOnce() {
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(defaultAttendance);
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns(defaultRawScore);

      _ = _calculator.CalculateCurrentScore(_student, _course);

      _mockAttendance.Verify(m => m.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(m => m.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.2:
    [Test]
    public void CalculateCurrentScore_WhenAssignmentsNull_CalculatesOnlyAttendance() {
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns((int?)null);
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(fullAttendance);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(fullAttendance));
    }

    // Проверка 3.3:
    [Test]
    public void CalculateTotalScore_CallsCalculateCurrentScore_DoesNotCallRepositoriesTwice() {
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Returns(totalScoreAttendance);
      _ = _mockAssignments.Setup(m => m.GetRawScore(_student, _course)).Returns(totalScoreRawScore);

      _ = _calculator.CalculateTotalScore(_student, _course);

      _mockAttendance.Verify(m => m.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(m => m.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.4:
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_ThrowsSameException() {
      _ = _mockAttendance.Setup(m => m.GetAttendedClasses(_student, _course)).Throws<TimeoutException>();

      _ = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student, _course));
    }
  }
}
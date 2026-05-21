using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class CurrentScoreCombinedTests {
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _student = new Student { Id = 1 };

      // Курс: Exam, MaxRaw = 600, TotalClasses = 20, MaxAttendance = 15
      // maxCurrent для Exam = 60 → maxAssignments = 60 - 15 = 45
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };
    }

    [TestCase(0, 0, 0)]
    [TestCase(50, 50, 30)]
    [TestCase(100, 0, 45)]
    [TestCase(0, 100, 15)]
    [TestCase(100, 100, 60)]
    [TestCase(30, 80, 25.5)]

    public void CalculateCurrentScore_CombinedVariousPercentages_ReturnsExpected(
        int rawPercent, int attendancePercent, double expectedCurrent) {
      // Arrange
      double rawScore = rawPercent / 100.0 * _course.MaxRawAssignmentsScore;
      int attendedClasses = (int)(attendancePercent / 100.0 * _course.TotalClasses);

      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(attendedClasses);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(rawScore);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      // Act
      double result = calculator.CalculateCurrentScore(_student, _course);

      // Assert
      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}

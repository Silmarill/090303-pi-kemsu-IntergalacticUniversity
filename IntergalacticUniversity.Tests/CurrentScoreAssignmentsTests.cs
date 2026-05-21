using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class CurrentScoreAssignmentsTests {
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _student = new Student { Id = 67 };

      // Курс: Exam, MaxRaw = 1000, MaxAttendance = 20 → maxAssignments = 40
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(500, 20)]
    [TestCase(700, 28)]
    [TestCase(1000, 40)]
    [TestCase(1200, 40)]

    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPart(
        double rawScore, double expectedAssignmentsScore) {
      // Arrange: фиксируем посещаемость 100%
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(rawScore);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double expectedAttendance = 20;
      double expectedTotal = expectedAssignmentsScore + expectedAttendance;

      // Act
      double result = calculator.CalculateCurrentScore(_student, _course);

      // Assert
      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}

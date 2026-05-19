using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_2_AssignmentsPortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();

      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(500, 20)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_VariousRawPercentages_ReturnsCorrectAssignmentsScore(
        int attended, double expectedAssignmentsScore) {
      // Arrange
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      double expected = expectedAssignmentsScore + 70;
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}
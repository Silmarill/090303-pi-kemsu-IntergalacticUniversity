using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_3_AttendancePortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 10
      };

      _mockAttendance = new Mock<IAttendanceRepository>();

      _mockAssignments = new Mock<IAssignmentsRepository>();
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(30, 10)]
    [TestCase(15, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendancePercentages_ReturnsCorrectScore(
        int attended, double expectedAttendanceScore) {
      // Arrange
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      double expected = expectedAttendanceScore + 70;
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}
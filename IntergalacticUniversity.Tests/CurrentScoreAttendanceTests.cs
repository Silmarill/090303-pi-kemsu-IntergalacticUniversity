using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class CurrentScoreAttendanceTests {
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _student = new Student { Id = 67 };

      // Курс: Credit, maxCurrent = 80, maxAttendance = 10 maxAssignments = 70
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };
    }

    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    [TestCase(30, 7.5)]
    [TestCase(10, 2.5)]

    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePart(
        int attendedClasses, double expectedAttendanceScore) {
      // Arrange: фиксируем задания на 100%
      _ = _mockAttendance.Setup(repo => repo.GetAttendedClasses(_student, _course)).Returns(attendedClasses);
      _ = _mockAssignments.Setup(repo => repo.GetRawScore(_student, _course)).Returns(1000);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      // Ожидаемые задания: 70 баллов (maxAssignments)
      double expectedAssignments = 70;
      double expectedTotal = expectedAssignments + expectedAttendanceScore;

      // Act
      double result = calculator.CalculateCurrentScore(_student, _course);

      // Assert
      Assert.That(result, Is.EqualTo(expectedTotal));
    }

  }
}

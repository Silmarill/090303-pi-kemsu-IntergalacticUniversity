using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_3_AttendancePortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
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

      mockAttendance = new Mock<IAttendanceRepository>();

      mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);

      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TestCase(30, 10)]
    [TestCase(15, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendancePercentages_ReturnsCorrectScore(
        int attended, double expectedAttendanceScore) {
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      double expected = expectedAttendanceScore + 70;
      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }
  }
}
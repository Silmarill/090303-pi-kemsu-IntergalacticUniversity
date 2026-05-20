using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_2_AssignmentsPortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
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

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(30);

      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(500, 20)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_VariousRawPercentages_ReturnsCorrectAssignmentsScore(
        double rawScore, double expectedAssignmentsScore) {
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      double expected = expectedAssignmentsScore + 20;
      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }
  }
}
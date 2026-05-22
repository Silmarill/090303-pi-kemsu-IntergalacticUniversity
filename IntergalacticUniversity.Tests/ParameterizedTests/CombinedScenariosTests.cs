using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenariosTests {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(10, 10.0)]
    [TestCase(5, 5.0)]
    [TestCase(0, 0.0)]
    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePart(
        int attended, double expectedAttendancePart) {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 10,
        MaxAttendanceScore = 10
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(70.0 + expectedAttendancePart));
    }

    [TestCase(0.0, 0, 0.0)]
    [TestCase(0.5, 10, 30.0)]
    [TestCase(1.0, 20, 60.0)]
    [TestCase(0.3, 20, 28.5)]
    public void CalculateCurrentScore_CombinedPercents_ReturnsExpected(
    double rawPercent, int attended, double expectedCurrent) {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawPercent * 600);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.01));
    }
  }
}
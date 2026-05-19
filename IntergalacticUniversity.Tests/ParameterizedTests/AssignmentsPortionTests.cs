using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
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
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0.0)]
    [TestCase(300, 12.0)]
    [TestCase(1000, 40.0)]
    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPart(
        double rawScore, double expectedAssignmentsPart) {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(expectedAssignmentsPart + 20.0));
    }
  }
}
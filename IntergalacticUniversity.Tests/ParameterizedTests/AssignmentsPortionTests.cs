using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void Setup() {
      _student = new Student { Id = 1 };

      double maxRawAssignmentsScore = 1800.0;
      int totalClasses = 30;
      int maxAttendanceScore = 20;

      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();

      int attendedFull = totalClasses;
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attendedFull);
    }

    [TestCase(0, 0)]
    [TestCase(540, 12)]
    [TestCase(1800, 40)]
    public void CalculateCurrentScore_DifferentRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsPart) {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent;
      double attendancePart = 20.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);

      calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(_student, _course);
      expectedCurrent = expectedAssignmentsPart + attendancePart;

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenarioTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void Setup() {
      int defaultStudentId = 1;
      _student = new Student { Id = defaultStudentId };

      double maxRaw = 600.0;
      int totalClasses = 20;
      int maxAttendance = 15;

      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [TestCase(0.0, 0.0, 0)]
    [TestCase(0.5, 0.5, 30)]
    [TestCase(1.0, 0.0, 45)]
    [TestCase(0.0, 1.0, 15)]
    [TestCase(1.0, 1.0, 60)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        double rawPercent, double attendancePercent, double expectedCurrent) {
      double delta = 0.001;
      double rawScore = rawPercent * _course.MaxRawAssignmentsScore;
      int attended = (int)(attendancePercent * _course.TotalClasses);

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      double actualCurrent = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent).Within(delta));
    }
  }
}
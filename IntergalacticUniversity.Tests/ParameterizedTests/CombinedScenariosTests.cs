using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenariosTests {
    private Student _student = null!;
    private Course _course = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15,
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0, 0)]
    [TestCase(50, 50, 30)]
    [TestCase(100, 100, 60)]
    [TestCase(30, 100, 28.5)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        int rawPercent,
        int attendancePercent,
        double expectedCurrent) {
      double rawScore = rawPercent / 100.0 * _course.MaxRawAssignmentsScore;
      int attended = (int)(attendancePercent / 100.0 * _course.TotalClasses);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(expectedCurrent));
    }
  }
}

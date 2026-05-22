using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenariosTests {
    private static readonly double MaxRawAssignments = 600.0;
    private static readonly int TotalClassesCount = 20;
    private static readonly int MaxAttendanceScore = 15;
    private static readonly double PercentScale = 100.0;

    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0, 0)]
    [TestCase(100, 100, 60)]
    [TestCase(50, 50, 30)]
    [TestCase(100, 0, 45)]
    public void CalculateCurrentScore_WhenPercentagesVary_ReturnsExpectedCurrent(
        int rawPercent,
        int attendancePercent,
        double expectedCurrent) {
      double rawScore = rawPercent / PercentScale * MaxRawAssignments;
      int attendedClasses = (int)(attendancePercent / PercentScale * TotalClassesCount);

      _ = _mockAssignments.Setup(repository => repository.GetRawScore(_student, _course))
          .Returns(rawScore);
      _ = _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, _course))
          .Returns(attendedClasses);

      double actualCurrent = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent).Within(TestDataFactory.ScoreTolerance));
    }
  }
}

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private const double RawScoreAtZeroPercent = 0.0;
    private const double RawScoreAtThirtyPercent = 300.0;
    private const double RawScoreAtFullPercent = 1000.0;
    private const double ExpectedCurrentAtZero = 20.0;
    private const double ExpectedCurrentAtThirty = 32.0;
    private const double ExpectedCurrentAtFull = 60.0;

    private static readonly double MaxRawAssignments = 1000.0;
    private static readonly int TotalClassesCount = 30;
    private static readonly int MaxAttendanceScore = 20;
    private static readonly int FullAttendanceCount = 30;

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
      _ = _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, _course))
          .Returns(FullAttendanceCount);

      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(RawScoreAtZeroPercent, ExpectedCurrentAtZero)]
    [TestCase(RawScoreAtThirtyPercent, ExpectedCurrentAtThirty)]
    [TestCase(RawScoreAtFullPercent, ExpectedCurrentAtFull)]
    public void CalculateCurrentScore_WhenAssignmentsVary_ReturnsExpectedCurrentWithFullAttendance(
        double rawScore,
        double expectedScore) {
      _ = _mockAssignments.Setup(repository => repository.GetRawScore(_student, _course))
          .Returns(rawScore);

      double actualScore = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualScore, Is.EqualTo(expectedScore).Within(TestDataFactory.ScoreTolerance));
    }
  }
}

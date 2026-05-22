using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTests {
    private const int AttendedAtFull = 20;
    private const int AttendedAtHalf = 10;
    private const int AttendedAtZero = 0;
    private const double ExpectedTotalAtFull = 80.0;
    private const double ExpectedTotalAtHalf = 75.0;
    private const double ExpectedTotalAtZero = 70.0;

    private static readonly double MaxRawAssignments = 1000.0;
    private static readonly int TotalClassesCount = 20;
    private static readonly int MaxAttendanceScore = 10;
    private static readonly double FullAssignmentsRaw = 1000.0;

    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateCreditCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);

      _mockAssignments = new Mock<IAssignmentsRepository>();
      _ = _mockAssignments.Setup(repository => repository.GetRawScore(_student, _course))
          .Returns(FullAssignmentsRaw);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(AttendedAtFull, ExpectedTotalAtFull)]
    [TestCase(AttendedAtHalf, ExpectedTotalAtHalf)]
    [TestCase(AttendedAtZero, ExpectedTotalAtZero)]
    public void CalculateCurrentScore_WhenAttendanceVary_ReturnsExpectedTotal(
        int attendedClasses,
        double expectedScore) {
      _ = _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, _course))
          .Returns(attendedClasses);

      double actualScore = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualScore, Is.EqualTo(expectedScore).Within(TestDataFactory.ScoreTolerance));
    }
  }
}

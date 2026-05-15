using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private static readonly double ExpectedZeroScore = 0.0;
    private static readonly double ExpectedFullExamCurrentScore = 60.0;
    private static readonly double MaxRawForFullExam = 800.0;
    private static readonly int TotalClassesForFullExam = 40;
    private static readonly int MaxAttendanceForFullExam = 20;
    private static readonly int AttendedForFullExam = 40;
    private static readonly double PerfectTotalScore = 100.0;
    private static readonly string ExpectedExcellentGrade = "Отлично";

    private static readonly double MaxRawForCreditCap = 1000.0;
    private static readonly double RawScoreAboveMaximum = 1200.0;
    private static readonly int TotalClassesForCreditCap = 40;
    private static readonly int MaxAttendanceForCreditCap = 15;
    private static readonly int AttendedForFullCredit = 40;
    private static readonly double ExpectedCreditCapScore = 80.0;

    private static readonly double MaxRawForCreditTotal = 1000.0;
    private static readonly int TotalClassesForCreditTotal = 20;
    private static readonly int MaxAttendanceForCreditTotal = 10;
    private static readonly int AttendedForHalfCredit = 10;
    private static readonly double FullAssignmentsRaw = 1000.0;
    private static readonly double ExpectedCurrentBeforeCredit = 75.0;
    private static readonly double MaxCreditScore = 20.0;
    private static readonly double ExpectedTotalWithCredit = 95.0;

    private static readonly double MaxRawForNoData = 1000.0;
    private static readonly int TotalClassesForNoData = 30;
    private static readonly int MaxAttendanceForNoData = 20;

    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      Course course = TestDataFactory.CreateExamCourse(
          MaxRawForNoData,
          TotalClassesForNoData,
          MaxAttendanceForNoData);

      _mockAssignments.Setup(repository => repository.GetRawScore(_student, course))
          .Returns((double?)null);
      _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, course))
          .Returns((int?)null);

      double actualScore = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(actualScore, Is.EqualTo(ExpectedZeroScore));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaximumAndExcellentGrade() {
      Course course = TestDataFactory.CreateExamCourse(
          MaxRawForFullExam,
          TotalClassesForFullExam,
          MaxAttendanceForFullExam);

      _mockAssignments.Setup(repository => repository.GetRawScore(_student, course))
          .Returns(MaxRawForFullExam);
      _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, course))
          .Returns(AttendedForFullExam);

      double actualScore = _calculator.CalculateCurrentScore(_student, course);
      string actualGrade = _calculator.ConvertToGrade(PerfectTotalScore);

      Assert.That(actualScore, Is.EqualTo(ExpectedFullExamCurrentScore));
      Assert.That(actualGrade, Is.EqualTo(ExpectedExcellentGrade));
    }

    [Test]
    public void CalculateCurrentScore_WhenOverMaximum_ReturnsCappedCreditScore() {
      Course course = TestDataFactory.CreateCreditCourse(
          MaxRawForCreditCap,
          TotalClassesForCreditCap,
          MaxAttendanceForCreditCap);

      _mockAssignments.Setup(repository => repository.GetRawScore(_student, course))
          .Returns(RawScoreAboveMaximum);
      _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, course))
          .Returns(AttendedForFullCredit);

      double actualScore = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(actualScore, Is.EqualTo(ExpectedCreditCapScore));
    }

    [Test]
    public void CalculateTotalScore_WhenCreditAtMaximum_ReturnsNinetyFive() {
      Course course = TestDataFactory.CreateCreditCourse(
          MaxRawForCreditTotal,
          TotalClassesForCreditTotal,
          MaxAttendanceForCreditTotal);

      _mockAssignments.Setup(repository => repository.GetRawScore(_student, course))
          .Returns(FullAssignmentsRaw);
      _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, course))
          .Returns(AttendedForHalfCredit);

      double currentScore = _calculator.CalculateCurrentScore(_student, course);
      Assert.That(currentScore, Is.EqualTo(ExpectedCurrentBeforeCredit));

      double actualTotal = _calculator.CalculateTotalScore(_student, course, MaxCreditScore);

      Assert.That(actualTotal, Is.EqualTo(ExpectedTotalWithCredit));
    }
  }
}

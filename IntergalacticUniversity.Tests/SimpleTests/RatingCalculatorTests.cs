using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class RatingCalculatorTests {
    private Student? _student;
    private Mock<IAttendanceRepository>? _mockAttendance;
    private Mock<IAssignmentsRepository>? _mockAssignments;
    private RatingCalculator? _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student {
        Id = 1,
        Name = "Тестовый Студент"
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();

      _calculator = new RatingCalculator(
        _mockAttendance.Object,
        _mockAssignments.Object
      );
    }

    [TearDown]
    public void TearDown() {
      _student = null;
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      Course course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, course))
          .Returns(null as int?);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, course))
          .Returns(null as double?);

      double result;
      result = _calculator!.CalculateCurrentScore(_student!, course);

      double expectedZeroScore;
      expectedZeroScore = 0.0;
      Assert.That(result, Is.EqualTo(expectedZeroScore));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrent() {
      int fullAttendance;
      fullAttendance = 40;

      double fullRawScore;
      fullRawScore = 800;

      double expectedExamMaxCurrent;
      expectedExamMaxCurrent = 60.0;

      Course course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, course))
          .Returns(fullAttendance);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, course))
          .Returns(fullRawScore);

      double currentScore;
      currentScore = _calculator!.CalculateCurrentScore(_student!, course);

      Assert.That(currentScore, Is.EqualTo(expectedExamMaxCurrent));
    }

    [Test]
    public void ConvertToGrade_WhenTotalScoreIs100_ReturnsExcellent() {
      double perfectScore;
      perfectScore = 100.0;

      string expectedExcellentGrade;
      expectedExcellentGrade = "Отлично";

      string grade;
      grade = _calculator!.ConvertToGrade(perfectScore);

      Assert.That(grade, Is.EqualTo(expectedExcellentGrade));
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreExceedsMax_ReturnsLimitedByMaxCurrent() {
      int fullAttendance;
      fullAttendance = 40;

      double excessiveRawScore;
      excessiveRawScore = 1200;

      double expectedCreditMaxCurrent;
      expectedCreditMaxCurrent = 80.0;

      Course course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, course))
          .Returns(fullAttendance);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, course))
          .Returns(excessiveRawScore);

      double currentScore;
      currentScore = _calculator!.CalculateCurrentScore(_student!, course);

      Assert.That(currentScore, Is.EqualTo(expectedCreditMaxCurrent));
    }

    [Test]
    public void CalculateTotalScore_WhenCreditAndCurrentScoreIs75AndCreditMax_Returns95() {
      int fullAttendance;
      fullAttendance = 40;

      double requiredRawScoreFor75Current;
      requiredRawScoreFor75Current = 60.0 / 65.0 * 1000;

      double maxCreditExamScore;
      maxCreditExamScore = 20.0;

      double expectedTotalScore;
      expectedTotalScore = 95.0;

      Course course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, course))
          .Returns(fullAttendance);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, course))
          .Returns(requiredRawScoreFor75Current);

      double totalScore;
      totalScore = _calculator!.CalculateTotalScore(_student!, course, maxCreditExamScore);

      Assert.That(totalScore, Is.EqualTo(expectedTotalScore));
    }
  }
}
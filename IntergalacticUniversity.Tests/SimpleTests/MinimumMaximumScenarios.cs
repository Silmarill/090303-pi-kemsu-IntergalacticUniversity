using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void Setup() {
      // DeepSeek: помог настроить инициализацию тестовых данных
      _student = new Student { Id = 1, Name = "Test Student" };

      _mockAttendance = new Mock<IAttendanceRepository>();

      _mockAssignments = new Mock<IAssignmentsRepository>();

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      Course course;
      double expectedCurrentScore;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      // DeepSeek: подсказал как правильно возвращать null через Mock
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns((int?)null);

      expectedCurrentScore = 0.0;
      double actual = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(actual, Is.EqualTo(expectedCurrentScore));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrentAndConvertToGrade_ReturnsExcellent() {
      Course course;
      double maxRawScore;
      int totalClasses;
      int maxAttendanceScore;
      double expectedCurrentScore;
      int perfectExamScore;
      string expectedGrade;

      maxRawScore = 800.0;
      totalClasses = 40;
      maxAttendanceScore = 20;
      expectedCurrentScore = 60.0;
      perfectExamScore = 100;
      expectedGrade = "Отлично";

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRawScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(maxRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(totalClasses);

      double actualCurrent = _calculator.CalculateCurrentScore(_student, course);
      string actualGrade = _calculator.ConvertToGrade(perfectExamScore);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrentScore));
      Assert.That(actualGrade, Is.EqualTo(expectedGrade));
    }

    [Test]
    public void CalculateCurrentScore_ForCreditCourse_WhenOverflow_DoesNotExceedMaxCurrent() {
      Course course;
      double maxRawScore;
      int totalClasses;
      int maxAttendanceScore;
      double rawScoreOverflow;
      int attendedFull;
      double maxCurrentForCredit;
      double actualCurrent;

      maxRawScore = 1000.0;
      totalClasses = 20;
      maxAttendanceScore = 15;
      rawScoreOverflow = 1200.0;
      attendedFull = totalClasses;
      maxCurrentForCredit = 80.0;

      course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScoreOverflow);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attendedFull);

      actualCurrent = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(actualCurrent, Is.LessThanOrEqualTo(maxCurrentForCredit));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_WhenFullCredit_ReturnsCappedSum() {
      Course course;
      double maxRawScore;
      int totalClasses;
      int maxAttendanceScore;
      double rawScoreFor75Current;
      int attendedFull;
      double creditMaxScore;
      double expectedTotal;

      maxRawScore = 1000.0;
      totalClasses = 20;
      maxAttendanceScore = 15;

      rawScoreFor75Current = 60.0 / 65.0 * 1000.0;

      attendedFull = totalClasses;
      creditMaxScore = 20.0;
      expectedTotal = 95.0;

      course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScoreFor75Current);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attendedFull);

      double actualTotal = _calculator.CalculateTotalScore(_student, course, examOrCreditScore: creditMaxScore);

      Assert.That(actualTotal, Is.EqualTo(expectedTotal).Within(1e-10));
    }
  }
}
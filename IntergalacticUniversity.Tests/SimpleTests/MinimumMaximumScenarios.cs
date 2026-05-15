using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student student;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
    private RatingCalculator calculator;

    [SetUp]
    public void Setup() {
      student = new Student { Id = 1, Name = "Test Student" };

      mockAttendance = new Mock<IAttendanceRepository>();

      mockAssignments = new Mock<IAssignmentsRepository>();

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
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

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      expectedCurrentScore = 0.0;
      double actual = calculator.CalculateCurrentScore(student, course);

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

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(maxRawScore);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(totalClasses);

      double actualCurrent = calculator.CalculateCurrentScore(student, course);
      string actualGrade = calculator.ConvertToGrade(perfectExamScore);

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

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScoreOverflow);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedFull);

      actualCurrent = calculator.CalculateCurrentScore(student, course);

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
      rawScoreFor75Current = 923.0;
      attendedFull = totalClasses;
      creditMaxScore = 20.0;
      expectedTotal = 95.0;

      course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScoreFor75Current);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedFull);

      double actualTotal = calculator.CalculateTotalScore(student, course, examOrCreditScore: creditMaxScore);

      Assert.That(actualTotal, Is.EqualTo(expectedTotal));
    }
  }
}
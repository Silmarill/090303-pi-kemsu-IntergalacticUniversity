using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock1 {
    private const double examMaxRawScore = 800;
    private const int examTotalClasses = 40;
    private const int examMaxAttendanceScore = 20;

    private const double creditMaxRawScore = 1000;
    private const int creditTotalClasses = 30;
    private const int creditMaxAttendanceScore = 15;
    private const int creditMaxCurrent = 80; // Лимит сверху для зачета

    private const double creditBonusPoints = 20; // Бонус за зачет

    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Геннадий Петров" };

      _examCourse = new Course {
        CourseId = 1,
        Name = "Математика",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = examMaxRawScore,
        TotalClasses = examTotalClasses,
        MaxAttendanceScore = examMaxAttendanceScore
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Программирование",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = creditMaxRawScore,
        TotalClasses = creditTotalClasses,
        MaxAttendanceScore = creditMaxAttendanceScore,
        MaxCurrent = creditMaxCurrent
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenAllNull_ReturnsZero() {
      // ИИ помог с обработкой null через GetValueOrDefault в основном коде
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);
      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WithMaxValues_ReturnsMaxCurrentForExam() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(examMaxRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(examTotalClasses);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);
      Assert.That(result, Is.EqualTo(60.0).Within(0.001));
    }

    [Test]
    public void ConvertToGrade_With100_ReturnsExcellent() {
      const int perfectScore = 100;
      string result = _calculator.ConvertToGrade(perfectScore);
      Assert.That(result, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMaxCurrent_CapsAtMaxCurrent() {
      const double overLimitRawScore = 1200; // Больше, чем creditMaxRawScore
      const int maxAttendanceForCredit = 30; // 100% посещений

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(overLimitRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(maxAttendanceForCredit);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);
      Assert.That(result, Is.EqualTo(creditMaxCurrent).Within(0.001));
    }

    [Test]
    public void CalculateTotalScore_WithCredit_AddsExamScoreCorrectly() {
      const double rawScoreForCredit = 875; // 87.5% выполнения
      const int attendedClassesForCredit = 30; // 100% посещений

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(rawScoreForCredit);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attendedClassesForCredit);

      double result = _calculator.CalculateTotalScore(_student, _creditCourse, creditBonusPoints);
      Assert.That(result, Is.EqualTo(95.0).Within(0.001));
    }
  }
}
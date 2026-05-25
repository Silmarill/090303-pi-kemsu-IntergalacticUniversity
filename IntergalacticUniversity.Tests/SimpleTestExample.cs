// SimpleTestExample.cs - Блок 1: Обычные тесты (4 проверки)
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleTestExample {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _examCourse = CreateExamCourse();
      _creditCourse = CreateCreditCourse();

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenMaxValues_Returns60AndGradeExcellent() {
      Course examCourse = CreateExamCourse(maxRaw: 80, totalClasses: 40, maxAttendance: 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, examCourse)).Returns(80);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, examCourse)).Returns(40);

      double currentScore = _calculator.CalculateCurrentScore(_student, examCourse);

      var calculatorForGrade = new RatingCalculator(
          new Mock<IAttendanceRepository>().Object,
          new Mock<IAssignmentsRepository>().Object);
      string grade = calculatorForGrade.ConvertToGrade(100);

      Assert.That(currentScore, Is.EqualTo(60.0).Within(0.001));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenSumExceedsMaxCurrent_CapsAtMaxCurrent() {
      Course creditCourse = CreateCreditCourse(maxRaw: 1000, totalClasses: 30, maxAttendance: 15);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(2000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(30);

      double result = _calculator.CalculateCurrentScore(_student, creditCourse);

      Assert.That(result, Is.EqualTo(80.0).Within(0.001));
      Assert.That(result, Is.LessThanOrEqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_WithMaxCredit_Returns95() {
      // Расчет: нужно получить currentScore = 75
      // maxAssignments = 65, maxAttendance = 15
      // attendancePortion = 15 (100% attendance)
      // rawScore = (75 - 15) / 65 * 100 = 60 / 65 * 100 ≈ 92.3077
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(92.3077);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      double totalScore = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      Assert.That(totalScore, Is.EqualTo(95.0).Within(0.1));
      Assert.That(totalScore, Is.LessThanOrEqualTo(100.0));
    }

    // Private helper methods - должны идти после всех public members
    private static Course CreateExamCourse(
        double maxRaw = 100,
        int totalClasses = 40,
        int maxAttendance = 20) {
      return new Course {
        CourseId = 1,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };
    }

    private static Course CreateCreditCourse(
        double maxRaw = 100,
        int totalClasses = 30,
        int maxAttendance = 15) {
      return new Course {
        CourseId = 2,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };
    }
  }
}
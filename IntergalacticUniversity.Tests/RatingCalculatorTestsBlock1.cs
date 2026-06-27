using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock1 {
    private const double ExamMaxRawScore = 800;
    private const int ExamTotalClasses = 40;
    private const int ExamMaxAttendanceScore = 20;

    private const double CreditMaxRawScore = 1000;
    private const int CreditTotalClasses = 30;
    private const int CreditMaxAttendanceScore = 15;

    private const double CreditBonusPoints = 20;

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
        MaxRawAssignmentsScore = ExamMaxRawScore,
        TotalClasses = ExamTotalClasses,
        MaxAttendanceScore = ExamMaxAttendanceScore
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Программирование",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = CreditMaxRawScore,
        TotalClasses = CreditTotalClasses,
        MaxAttendanceScore = CreditMaxAttendanceScore
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenAllNull_ReturnsZero() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);
      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WithMaxValues_ReturnsMaxCurrentForExam() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(ExamMaxRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(ExamTotalClasses);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);
      Assert.That(result, Is.EqualTo(60.0).Within(0.001));
    }

    [Test]
    public void ConvertToGrade_With100_ReturnsExcellent() {
      const int PerfectScore = 100;
      string result = _calculator.ConvertToGrade(PerfectScore);
      Assert.That(result, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMaxCurrent_CapsAtMaxCurrent() {
      const double OverLimitRawScore = 1200;
      const int MaxAttendanceForCredit = 30;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(OverLimitRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(MaxAttendanceForCredit);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);
      Assert.That(result, Is.EqualTo(80.0).Within(0.001));
    }

    // Исправленный тест 1.4
    [Test]
    public void CalculateTotalScore_WithCredit_AddsExamScoreCorrectly() {
      // Ручной расчет по подсказке преподавателя:
      // 1. При полной посещаемости (30/30) студент получает 15 баллов (MaxAttendanceScore = 15).
      // 2. Для текущего балла 75 нужно, чтобы задания дали 60 баллов (75 - 15 = 60).
      // 3. Максимум за задания = 80 - 15 = 65.
      // 4. Решаем уравнение: rawScore / 1000 * 65 = 60
      // 5. rawScore = 60 * 1000 / 65 ≈ 923.0769230769231

      const double RawScoreForCredit = 923.0769230769231;
      const int AttendedClassesForCredit = 30;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(RawScoreForCredit);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(AttendedClassesForCredit);

      double result = _calculator.CalculateTotalScore(_student, _creditCourse, CreditBonusPoints);

      // 75 (текущий) + 20 (бонус) = 95
      Assert.That(result, Is.EqualTo(95.0).Within(0.001));
    }
  }
}
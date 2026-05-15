using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock1 {
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
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Программирование",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    // Проверка 1.1: Минимальные значения – всё на нуле
    [Test]
    public void CalculateCurrentScore_WhenAllNull_ReturnsZero() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0).Within(0.001));
    }

    // Проверка 1.2: Максимальные значения
    [Test]
    public void CalculateCurrentScore_WithMaxValues_ReturnsMaxCurrentForExam() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(800);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(60).Within(0.001));
    }

    [Test]
    public void ConvertToGrade_With100_ReturnsExcellent() {
      string result = _calculator.ConvertToGrade(100);
      Assert.That(result, Is.EqualTo("Отлично"));
    }

    // Проверка 1.3: Ограничение сверху
    [Test]
    public void CalculateCurrentScore_WhenExceedsMaxCurrent_CapsAtMaxCurrent() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1200);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(80).Within(0.001));
    }

    // Проверка 1.4: Итоговый балл с зачётом
    [Test]
    public void CalculateTotalScore_WithCredit_AddsExamScoreCorrectly() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(875);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      double result = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      Assert.That(result, Is.EqualTo(95).Within(0.001));
    }
  }
}
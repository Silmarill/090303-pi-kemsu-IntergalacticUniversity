using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock2 {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "На тесте студент" };

      _examCourse = new Course {
        CourseId = 1,
        Name = "Физика",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Базы данных",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.1: Границы перевода баллов в оценку
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(int score, string expectedGrade) {
      string result = _calculator.ConvertToGrade(score);
      Assert.That(result, Is.EqualTo(expectedGrade));
    }

    // Проверка 2.2: Параметризация баллов за задания
    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_DifferentAssignmentPercentages_ReturnsCorrectAssignmentPart(
        double rawScore, double expectedAssignmentScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      double expected = expectedAssignmentScore + 20;
      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    // Проверка 2.3: Параметризация посещаемости
    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendancePercentages_ReturnsCorrectAttendancePart(
        int attendedClasses, double expectedAttendanceScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attendedClasses);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      double expected = 70 + expectedAttendanceScore;
      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    // Проверка 2.4: Комбинированный тест
    [TestCase(0, 0, 0)]
    [TestCase(50, 50, 22.5)]
    [TestCase(100, 100, 60)]
    [TestCase(100, 0, 40)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        int rawPercent, int attendancePercent, double expectedCurrent) {
      var customCourse = new Course {
        CourseId = 3,
        Name = "Комбинированный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      double rawScore = (rawPercent / 100.0) * 600;
      int attended = (attendancePercent * 20) / 100;

      _mockAssignments.Setup(r => r.GetRawScore(_student, customCourse)).Returns(rawScore);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, customCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, customCourse);
      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
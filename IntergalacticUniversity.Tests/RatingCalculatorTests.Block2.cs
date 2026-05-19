using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock2 {
    // Конфигурация экзаменационного курса
    private const double examMaxRawScore = 1000;
    private const int examTotalClasses = 40;
    private const int examMaxAttendanceScore = 20;

    // Конфигурация зачетного курса
    private const double creditMaxRawScore = 1000;
    private const int creditTotalClasses = 40;
    private const int creditMaxAttendanceScore = 10;
    private const int creditMaxCurrent = 80; // Лимит сверху для зачета

    // Параметры для комбинированного теста
    private const int customExamMaxRawScore = 600;
    private const int customExamTotalClasses = 20;
    private const int customExamMaxAttendanceScore = 15;

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
        MaxRawAssignmentsScore = examMaxRawScore,
        TotalClasses = examTotalClasses,
        MaxAttendanceScore = examMaxAttendanceScore
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Базы данных",
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

    // Проверка 2.1:
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

    // Проверка 2.2:
    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_DifferentAssignmentPercentages_ReturnsCorrectAssignmentPart(
        double rawScore, double expectedAssignmentScore) {
      const int maxAttendance = 40; // 100% посещений

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(maxAttendance);

      double expectedTotal = expectedAssignmentScore + examMaxAttendanceScore;
      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }

    // Проверка 2.3: Параметризация посещаемости
    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendancePercentages_ReturnsCorrectAttendancePart(
        int attendedClasses, double expectedAttendanceScore) {
      const double maxRawScore = 1000; // 100% выполнения заданий

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(maxRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attendedClasses);

      const double assignmentMaxScoreForCredit = 70;
      double expectedTotal = assignmentMaxScoreForCredit + expectedAttendanceScore;
      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }

    // Проверка 2.4: Комбинированный тест
    [TestCase(0, 0, 0)]
    [TestCase(50, 50, 22.5)]
    [TestCase(100, 100, 60)]
    [TestCase(100, 0, 40)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        int rawPercent, int attendancePercent, double expectedCurrent) {
      // ИИ помог: Создание кастомного курса с именованными константами
      Course customCourse = new Course {
        CourseId = 3,
        Name = "Комбинированный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = customExamMaxRawScore,
        TotalClasses = customExamTotalClasses,
        MaxAttendanceScore = customExamMaxAttendanceScore
      };

      // Расчет значений через проценты
      double rawScore = rawPercent / 100.0 * customExamMaxRawScore;

      // ИИ помог: Расчет attended через double, чтобы избежать потери точности
      // при целочисленном делении (20 * 50 / 100 = 10, а не 0)
      int attended = (int)(attendancePercent / 100.0 * customExamTotalClasses);

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, customCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, customCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, customCourse);
      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
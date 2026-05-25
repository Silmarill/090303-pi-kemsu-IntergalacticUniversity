// TestCaseExample.cs - Блок 2: Параметризованные тесты (4 проверки)
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class TestCaseExample {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.1: Границы перевода баллов в оценку
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(50, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(65, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(85, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      string grade = calculator.ConvertToGrade(totalScore);

      Assert.That(grade, Is.EqualTo(expectedGrade));
    }

    // Проверка 2.2: Параметризация приведения баллов за задания
    [TestCase(0, 0, "0% raw -> вклад заданий = 0")]
    [TestCase(3000, 12, "30% raw -> вклад заданий = 12")]
    [TestCase(5000, 20, "50% raw -> вклад заданий = 20")]
    [TestCase(7000, 28, "70% raw -> вклад заданий = 28")]
    [TestCase(10000, 40, "100% raw -> вклад заданий = 40")]
    [TestCase(15000, 40, "150% raw -> ограничение максимумом 40")]
    public void CalculateCurrentScore_DifferentRawScores_ReturnsCorrectAssignmentsPortion(
        double rawScore, double expectedAssignmentsPortion, string description) {
      // Курс: Exam, maxCurrent=60, maxAttendance=20 -> maxAssignments=40
      Course examCourse = new Course {
        CourseId = 1,
        Name = "Экзамен",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 10000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, examCourse)).Returns(40); // 100% attendance

      double result = _calculator.CalculateCurrentScore(_student, examCourse);

      // Ожидаем: вклад заданий + вклад посещаемости (20)
      Assert.That(result, Is.EqualTo(expectedAssignmentsPortion + 20).Within(0.001), description);
    }

    // Проверка 2.3: Параметризация учёта посещаемости
    [TestCase(30, 10, "100% посещаемости -> вклад = 10")]
    [TestCase(15, 5, "50% посещаемости -> вклад = 5")]
    [TestCase(0, 0, "0% посещаемости -> вклад = 0")]
    [TestCase(20, 6.666666666666667, "67% посещаемости -> вклад = 6.67")]
    public void CalculateCurrentScore_DifferentAttendance_ReturnsCorrectAttendancePortion(
        int attendedClasses, double expectedAttendancePortion, string description) {
      // Курс: Credit, maxCurrent=80, maxAttendance=10 -> maxAssignments=70
      Course creditCourse = new Course {
        CourseId = 2,
        Name = "Зачёт",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 30,
        MaxAttendanceScore = 10
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(100); // 100% заданий
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(attendedClasses);

      double result = _calculator.CalculateCurrentScore(_student, creditCourse);

      // Ожидаем: вклад заданий (70) + вклад посещаемости
      Assert.That(result, Is.EqualTo(70 + expectedAttendancePortion).Within(0.001), description);
    }

    // Проверка 2.4: Комбинированный параметризованный тест
    [TestCase(0, 0, 0, "0% заданий, 0% посещаемости -> 0")]
    [TestCase(300, 10, 30, "50% заданий (22.5), 50% посещаемости (7.5) -> 30")]
    [TestCase(600, 20, 60, "100% заданий (45), 100% посещаемости (15) -> 60")]
    [TestCase(150, 5, 15, "25% заданий (11.25), 25% посещаемости (3.75) -> 15")]
    [TestCase(600, 0, 45, "100% заданий (45), 0% посещаемости (0) -> 45")]
    [TestCase(0, 20, 15, "0% заданий (0), 100% посещаемости (15) -> 15")]
    public void CalculateCurrentScore_CombinedScenarios_ReturnsExpectedScore(
        double rawScore, int attendedClasses, double expectedScore, string description) {
      // Курс: Exam, maxRaw=600, totalClasses=20, maxAttendance=15
      // maxCurrent = 60, maxAssignments = 45
      // Расчеты:
      // - Вклад заданий = (rawScore / maxRaw) * maxAssignments
      // - Вклад посещаемости = (attendedClasses / totalClasses) * maxAttendance
      // - Итог = сумма (ограничивается maxCurrent = 60)
      Course course = new Course {
        CourseId = 3,
        Name = "Комбинированный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attendedClasses);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expectedScore).Within(0.001), description);
    }
  }
}
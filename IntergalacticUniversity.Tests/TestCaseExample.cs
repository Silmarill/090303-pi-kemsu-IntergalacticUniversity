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
      // Создаём моки для конструктора
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      string grade = calculator.ConvertToGrade(totalScore);

      // Assert
      Assert.That(grade, Is.EqualTo(expectedGrade));
    }

    // Проверка 2.2: Параметризация приведения баллов за задания
    [TestCase(0, 0)]
    [TestCase(3000, 12)]
    [TestCase(5000, 20)]
    [TestCase(7000, 28)]
    [TestCase(10000, 40)]
    [TestCase(15000, 40)]
    public void CalculateCurrentScore_DifferentRawScores_ReturnsCorrectAssignmentsPortion(
        double rawScore, double expectedAssignmentsPortion) {
      // Arrange: Exam course, maxCurrent=60, maxAttendance=20 -> maxAssignments=40
      Course examCourse = new Course {
        CourseId = 1,
        Name = "Экзамен",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 10000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, examCourse)).Returns(40);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, examCourse);

      // Assert: assignmentsPortion + 20 (attendance)
      Assert.That(result, Is.EqualTo(expectedAssignmentsPortion + 20).Within(0.001));
    }

    // Проверка 2.3: Параметризация учёта посещаемости
    [TestCase(30, 10)]
    [TestCase(15, 5)]
    [TestCase(0, 0)]
    [TestCase(20, 6.666666666666667)]
    public void CalculateCurrentScore_DifferentAttendance_ReturnsCorrectAttendancePortion(
        int attendedClasses, double expectedAttendancePortion) {
      // Arrange: Credit course, maxCurrent=80, maxAttendance=10 -> maxAssignments=70
      Course creditCourse = new Course {
        CourseId = 2,
        Name = "Зачёт",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 30,
        MaxAttendanceScore = 10
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(100);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(attendedClasses);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, creditCourse);

      // Assert: assignments=70 + attendancePortion
      Assert.That(result, Is.EqualTo(70 + expectedAttendancePortion).Within(0.001));
    }

    // Проверка 2.4: Комбинированный параметризованный тест
    [TestCase(0, 0, 0)]
    [TestCase(300, 10, 30)]
    [TestCase(600, 20, 60)]
    [TestCase(150, 5, 11.25)]
    [TestCase(600, 0, 45)]
    [TestCase(0, 20, 15)]
    public void CalculateCurrentScore_CombinedScenarios_ReturnsExpectedScore(
        double rawScore, int attendedClasses, double expectedScore) {
      // Arrange: Exam course, maxRaw=600, totalClasses=20, maxAttendance=15
      // maxCurrent=60, maxAssignments=45
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

      // Act
      double result = _calculator.CalculateCurrentScore(_student, course);

      // Assert
      Assert.That(result, Is.EqualTo(expectedScore).Within(0.001));
    }
  }
}
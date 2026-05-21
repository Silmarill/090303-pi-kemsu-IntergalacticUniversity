using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();
      _examCourse = TestDataFactory.CreateExamCourse(maxRawAssignmentsScore: 800, totalClasses: 40, maxAttendanceScore: 20);
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance = null;
      _mockAssignments = null;
    }

    // Проверка 1.1: минимальные значения – всё на нуле
    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    // Проверка 1.2: максимальные значения – 100% заданий и 100% посещаемости
    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrent() {
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns(800);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double current = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(current, Is.EqualTo(60.0).Within(0.001));
    }

    [Test]
    public void ConvertToGrade_WhenTotalScore100_ReturnsExcellent() {
      string grade = _calculator.ConvertToGrade(100);

      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    // Проверка 1.3: проверка ограничения сверху – сумма не может превысить maxCurrent
    [Test]
    public void CalculateCurrentScore_WhenAssignmentsExceedMax_IsCappedAtMaxCurrent() {
      // maxCurrent = 80, maxAttendance = 15 -> maxAssignments = 65
      Course creditCourse = TestDataFactory.CreateCreditCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 15);

      // rawScore больше MaxRawAssignmentsScore (1200 > 1000) -> 120%
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, creditCourse)).Returns(1200);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, creditCourse)).Returns(40); // 100%

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, creditCourse);

      // maxCurrent для зачёта - 80
      Assert.That(result, Is.EqualTo(80.0).Within(0.001));
    }

    // Проверка 1.4: итоговый балл с зачётом – правильное сложение
    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCredit_ReturnsCorrectTotal() {
      Course creditCourse = TestDataFactory.CreateCreditCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 15);

      // Входные данные
      double rawScore = 1000;  // 100% заданий
      int attended = 27;       // 27 посещений из 40
      double creditScore = 20; // Максимальный балл за зачёт

      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, creditCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, creditCourse)).Returns(attended);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      // Вычисление expected по тем же формулам, что и в RatingCalculator
      // Для зачёта: maxCurrent = 80, maxAttendance = 15, maxAssignments = 65
      int maxCurrent = 80;
      int maxAttendance = creditCourse.MaxAttendanceScore;
      int maxAssignments = maxCurrent - maxAttendance;

      // Расчёт баллов за задания (с ограничением)
      double expectedAssignmentsScore = rawScore / creditCourse.MaxRawAssignmentsScore * maxAssignments;
      expectedAssignmentsScore = Math.Min(expectedAssignmentsScore, maxAssignments);  // = 65

      // Расчёт баллов за посещаемость
      double attendancePercent = (double)attended / creditCourse.TotalClasses;        // 27/40 = 0.675
      double expectedAttendanceScore = attendancePercent * maxAttendance;             // 0.675*15 = 10.125

      // Ожидаемая текущая успеваемость
      double expectedCurrentScore = expectedAssignmentsScore + expectedAttendanceScore;  // 65+10.125 = 75.125

      // Ожидаемый итоговый балл (с ограничением 100)
      double expectedTotalScore = expectedCurrentScore + Math.Min(creditScore, 20.0);     // 75.125+20 = 95.125
      expectedTotalScore = Math.Min(expectedTotalScore, 100.0);                          // 95.125

      double currentScore = calculator.CalculateCurrentScore(_student, creditCourse);
      double totalScore = calculator.CalculateTotalScore(_student, creditCourse, examOrCreditScore: 20);

      Assert.That(currentScore, Is.EqualTo(expectedCurrentScore).Within(0.001));
      Assert.That(totalScore, Is.EqualTo(expectedTotalScore).Within(0.001));
    }
  }
}

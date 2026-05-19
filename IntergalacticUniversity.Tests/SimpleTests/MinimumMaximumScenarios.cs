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

      Assert.That(result, Is.EqualTo(0.0));
    }

    // Проверка 1.2: максимальные значения – 100% заданий и 100% посещаемости
    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrent() {
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns(800);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double current = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(current, Is.EqualTo(60.0));
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
      Assert.That(result, Is.EqualTo(80.0));
    }

    // Проверка 1.4: итоговый балл с зачётом – правильное сложение
    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCredit_ReturnsCorrectTotal() {
      Course creditCourse = TestDataFactory.CreateCreditCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 15);

      // Текущая успеваемость: 75 баллов из 80
      // Для этого: задания - 65 баллов (100%), посещаемость - 10 баллов из 15
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, creditCourse)).Returns(1000);
      // 10 посещений из 40 - 25%, 0.25 * 15 = 3.75 балла - не подходит
      // Нужно подобрать attendanceScore = 10
      // percent = attendanceScore/maxAttendance = 10/15 = 0.666...
      // attended = percent * totalClasses = 0.666... * 40 ≈ 27
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, creditCourse)).Returns(27);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double currentScore = calculator.CalculateCurrentScore(_student, creditCourse);

      double totalScore = calculator.CalculateTotalScore(_student, creditCourse, examOrCreditScore: 20);

      Assert.That(totalScore, Is.EqualTo(95.0));
      Assert.That(currentScore, Is.EqualTo(75.0));
    }
  }
}

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test Student" };

      _examCourse = new Course {
        CourseId = 101,
        Name = "Exam Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _creditCourse = new Course {
        CourseId = 102,
        Name = "Credit Course",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

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
    public void CalculateCurrentScore_WhenMaxValues_ReturnsMaxCurrent() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(800);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(60.0).Within(0.001));
    }

    [Test]
    public void ConvertToGrade_WhenTotalScoreIs100_ReturnsExcellent() {
      string result = _calculator.ConvertToGrade(100);

      Assert.That(result, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMaxCurrent_CapsAtMaxCurrent() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1200);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(80.0).Within(0.001));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCurrentAndMaxExam_Returns95() {
      // Зачётный курс: maxCurrent = 80, maxAttendance = 15, maxAssignments = 65.
      // assignments = (raw / MaxRaw) * 65, attendance = (attended / TotalClasses) * 15.
      // current = assignments + attendance; total = min(current + min(credit, 20), 100).
      const double assignmentsRawScore = 60000.0 / 65.0; // 60 из 65 баллов заданий

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(assignmentsRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(40);

      double currentScore = _calculator.CalculateCurrentScore(_student, _creditCourse);
      double totalScore = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      Assert.That(currentScore, Is.EqualTo(75.0).Within(0.001));
      Assert.That(totalScore, Is.EqualTo(95.0).Within(0.001));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCurrentAndMaxCredit_Returns100() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(40);

      double currentScore = _calculator.CalculateCurrentScore(_student, _creditCourse);
      double totalScore = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      Assert.That(currentScore, Is.EqualTo(80.0).Within(0.001));
      Assert.That(totalScore, Is.EqualTo(100.0).Within(0.001));
    }
  }
}
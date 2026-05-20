using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student = null!;
    private Course _examCourse = null!;
    private Course _creditCourse = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _examCourse = new Course {
        CourseId = 1,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
      _creditCourse = new Course {
        CourseId = 2,
        Name = "Зачётный курс",
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
      _mockAttendance = null!;
      _mockAssignments = null!;
      _calculator = null!;
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarksForExam_ReturnsMaxCurrent() {
      double fullRawScore = _examCourse.MaxRawAssignmentsScore;
      int fullAttendance = _examCourse.TotalClasses;
      double expectedCurrent = 60.0;
      double fullTotalScore = 100.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(fullRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(fullAttendance);

      double currentScore = _calculator.CalculateCurrentScore(_student, _examCourse);
      string grade = _calculator.ConvertToGrade(fullTotalScore);

      Assert.That(currentScore, Is.EqualTo(expectedCurrent).Within(0.001));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMax_ReturnsMaxCurrent() {
      double exceedingRawScore = _creditCourse.MaxRawAssignmentsScore * 1.2;
      int fullAttendance = _creditCourse.TotalClasses;
      double expectedMaxCurrent = 80.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(exceedingRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(fullAttendance);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(expectedMaxCurrent).Within(0.001));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCredit_Returns95() {
      double maxAssignments = 80.0 - _creditCourse.MaxAttendanceScore;
      double neededAssignmentsPart = 75.0 - _creditCourse.MaxAttendanceScore;
      double rawFor75Current = neededAssignmentsPart / maxAssignments * _creditCourse.MaxRawAssignmentsScore;
      int fullAttendance = _creditCourse.TotalClasses;
      double maxCreditScore = 20.0;
      double expectedCurrent = 75.0;
      double expectedTotal = 95.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(rawFor75Current);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(fullAttendance);

      double current = _calculator.CalculateCurrentScore(_student, _creditCourse);
      double total = _calculator.CalculateTotalScore(_student, _creditCourse, maxCreditScore);

      Assert.That(current, Is.EqualTo(expectedCurrent).Within(0.001));
      Assert.That(total, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}
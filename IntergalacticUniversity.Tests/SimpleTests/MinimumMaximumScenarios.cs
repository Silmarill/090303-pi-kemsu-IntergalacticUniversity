using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {

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
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarksForExam_ReturnsMaxCurrent() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(800);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double currentScore = _calculator.CalculateCurrentScore(_student, _examCourse);
      string grade = _calculator.ConvertToGrade(100);

      Assert.That(currentScore, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMax_ReturnsMaxCurrent() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1200);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourseWithMaxCredit_Returns95() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(923.08);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(30);

      double current = _calculator.CalculateCurrentScore(_student, _creditCourse);
      double total = _calculator.CalculateTotalScore(_student, _creditCourse, 20);

      Assert.That(current, Is.EqualTo(75.0).Within(0.01));
      Assert.That(total, Is.EqualTo(95.0).Within(0.01));
    }
  }
}
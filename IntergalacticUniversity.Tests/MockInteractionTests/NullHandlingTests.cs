using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class NullHandlingTests {
    private Student _student = null!;
    private Course _course = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _course = new Course {
        CourseId = 1,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      int fullAttendance = _course.TotalClasses;
      double expectedAttendanceScore = _course.MaxAttendanceScore;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(fullAttendance);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(expectedAttendanceScore).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendedIsNull_ReturnsOnlyAssignmentsScore() {
      double fullRawScore = _course.MaxRawAssignmentsScore;
      int maxCurrentForExam = 60;
      double expectedAssignmentsScore = maxCurrentForExam - _course.MaxAttendanceScore;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(fullRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(expectedAssignmentsScore).Within(0.001));
    }
  }
}
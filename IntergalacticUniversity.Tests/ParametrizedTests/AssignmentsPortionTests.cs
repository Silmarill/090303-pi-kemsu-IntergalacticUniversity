using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class AssignmentsPortionTests {
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
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      _creditCourse = new Course {
        CourseId = 2,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsScore) {
      int fullAttendance = _examCourse.TotalClasses;
      double maxAttendanceScore = _examCourse.MaxAttendanceScore;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(fullAttendance);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);
      double expected = expectedAssignmentsScore + maxAttendanceScore;

      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendanceScore) {
      double fullRawScore = _creditCourse.MaxRawAssignmentsScore;
      double maxAssignments = 70.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(fullRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);
      double expected = maxAssignments + expectedAttendanceScore;

      Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }
  }
}
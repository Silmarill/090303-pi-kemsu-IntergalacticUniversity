using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
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
      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
    }

    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments() {
      int halfAttendance = _course.TotalClasses / 2;
      double halfRawScore = _course.MaxRawAssignmentsScore / 2;

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(halfAttendance);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(halfRawScore);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      int fullAttendance = _course.TotalClasses;
      double fullRawScore = _course.MaxRawAssignmentsScore;
      double examScore = 30.0;

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(fullAttendance);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(fullRawScore);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }
  }
}
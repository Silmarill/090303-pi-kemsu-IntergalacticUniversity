using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class ExceptionPropagationTests {
    private Student _student = null!;
    private Course _course = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;

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
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_PropagatesException() {
      int fullAttendance = _course.TotalClasses;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Throws<TimeoutException>();
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(fullAttendance);

      _ = Assert.Throws<TimeoutException>(() => new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object).CalculateCurrentScore(_student, _course));
    }
  }
}
using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

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
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(400);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _calculator.CalculateCurrentScore(_student, _course);

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(800);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _calculator.CalculateTotalScore(_student, _course, 30);

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }
  }
}
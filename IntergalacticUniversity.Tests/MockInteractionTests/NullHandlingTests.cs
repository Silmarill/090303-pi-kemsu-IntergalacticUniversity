using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class NullHandlingTests {
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
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(20.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendedIsNull_ReturnsOnlyAssignmentsScore() {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(800);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(40.0));
    }
  }
}
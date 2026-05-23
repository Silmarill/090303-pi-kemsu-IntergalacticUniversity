using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class SingleCallVerificationTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      int fullAttendance;
      fullAttendance = 40;

      double fullRawScore;
      fullRawScore = 800;

      _student = new Student {
        Id = 1,
        Name = "Тестовый Студент"
      };

      _course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();

      _ = _mockAttendance
          .Setup(r => r.GetAttendedClasses(It.IsAny<Student>(), It.IsAny<Course>()))
          .Returns(fullAttendance);

      _ = _mockAssignments
          .Setup(r => r.GetRawScore(It.IsAny<Student>(), It.IsAny<Course>()))
          .Returns(fullRawScore);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      double examScore;
      examScore = 30.0;

      _ = _calculator.CalculateTotalScore(_student, _course, examScore);

      _mockAttendance.Verify(
          r => r.GetAttendedClasses(_student, _course),
          Times.Once);

      _mockAssignments.Verify(
          r => r.GetRawScore(_student, _course),
          Times.Once);
    }
  }
}
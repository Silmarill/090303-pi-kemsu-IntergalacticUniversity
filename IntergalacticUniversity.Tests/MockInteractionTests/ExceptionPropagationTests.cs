using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test Student" };
      _course = new Course {
        CourseId = 101,
        Name = "Test Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course))
          .Throws<TimeoutException>();
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      Assert.That(
          () => calculator.CalculateCurrentScore(_student, _course),
          Throws.TypeOf<TimeoutException>());
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_PropagatesException() {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course))
          .Throws<TimeoutException>();

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      Assert.That(
          () => calculator.CalculateCurrentScore(_student, _course),
          Throws.TypeOf<TimeoutException>());
    }

    [Test]
    public void CalculateTotalScore_WhenCalledOnce_CallsRepositoriesOnce() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(_student, _course, 30);

      mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }
  }
}
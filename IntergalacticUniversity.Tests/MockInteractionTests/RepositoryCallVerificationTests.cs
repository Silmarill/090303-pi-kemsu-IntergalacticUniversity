using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

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

      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_CallsRepositoriesExactlyOnce() {
      _calculator.CalculateCurrentScore(_student, _course);

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }
  }
}
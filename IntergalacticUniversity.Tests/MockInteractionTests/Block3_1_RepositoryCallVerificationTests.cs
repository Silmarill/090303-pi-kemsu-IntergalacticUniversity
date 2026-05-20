using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class Block3_1_RepositoryCallVerificationTests {
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesExactlyOnce() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      _ = _mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1000);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = calculator.CalculateCurrentScore(student, course);

      _mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }
  }
}
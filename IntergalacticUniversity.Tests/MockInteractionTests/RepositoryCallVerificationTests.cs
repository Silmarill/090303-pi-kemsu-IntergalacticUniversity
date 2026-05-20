using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    [Test]
    public void CalculateCurrentScore_ShouldCallRepositoriesExactlyOnce_WithCorrectArguments() {
      Student student = new Student { Id = 42, Name = "Иван Поверенный" };
      Course course = new Course { Type = ExamType.Exam, MaxRawAssignmentsScore = 100, TotalClasses = 10, MaxAttendanceScore = 10 };

      Mock<IAttendanceRepository> mockAttend = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssign = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssign.Setup(repo => repo.GetRawScore(student, course)).Returns(50.0);
      _ = mockAttend.Setup(repo => repo.GetAttendedClasses(student, course)).Returns(5);

      RatingCalculator calculator = new RatingCalculator(mockAttend.Object, mockAssign.Object);

      _ = calculator.CalculateCurrentScore(student, course);

      mockAssign.Verify(repo => repo.GetRawScore(student, course), Times.Once);
      mockAttend.Verify(repo => repo.GetAttendedClasses(student, course), Times.Once);
    }
  }
}
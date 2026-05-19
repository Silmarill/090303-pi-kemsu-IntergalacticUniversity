using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class PerformanceVerificationTests {
    [Test]
    public void CalculateTotalScore_ShouldNotCallRepositoriesRepeatedly() {
      Student student = new Student { Id = 1 };
      Course course = new Course { Type = ExamType.Exam, MaxRawAssignmentsScore = 100, TotalClasses = 10, MaxAttendanceScore = 10 };

      Mock<IAttendanceRepository> mockAttend = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssign = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssign.Setup(r => r.GetRawScore(student, course)).Returns(50.0);
      _ = mockAttend.Setup(r => r.GetAttendedClasses(student, course)).Returns(5);

      RatingCalculator calculator = new RatingCalculator(mockAttend.Object, mockAssign.Object);

      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);

      mockAssign.Verify(r => r.GetRawScore(student, course), Times.Once);
      mockAttend.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
    }
  }
}
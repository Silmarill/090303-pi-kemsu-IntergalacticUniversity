using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsTimeoutException_ThrowsSameException() {
      Student student = new Student { Id = 1 };
      Course course = new Course { Type = ExamType.Exam };

      Mock<IAssignmentsRepository> mockAssign = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAssign.Setup(mock => mock.GetRawScore(It.IsAny<Student>(), It.IsAny<Course>())).Throws<TimeoutException>();

      Mock<IAttendanceRepository> mockAttend = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      RatingCalculator calculator = new RatingCalculator(mockAttend.Object, mockAssign.Object);

      _ = Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(student, course));
    }
  }
}
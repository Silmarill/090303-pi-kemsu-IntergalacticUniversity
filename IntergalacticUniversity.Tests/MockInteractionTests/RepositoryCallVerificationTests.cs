using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments_OnceEach() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(15);
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500.0);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      calculator.CalculateCurrentScore(student, course);

      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }
  }
}
using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    // Проверка 3.1 - репозитории вызываются ровно один раз с правильными аргументами
    [Test]
    public void CalculateCurrentScore_CallsEachRepositoryExactlyOnceWithCorrectArgs() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 500,
        TotalClasses = 20,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(10);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(250.0);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateCurrentScore(student, course);

      // Проверяем что каждый репозиторий был вызван ровно один раз с нужными объектами
      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }
  }
}

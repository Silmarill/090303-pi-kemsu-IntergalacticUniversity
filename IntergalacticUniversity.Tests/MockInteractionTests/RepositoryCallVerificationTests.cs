using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments_OnceEach() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;
      double rawScore;
      int attendedClasses;
      double maxRaw;
      int totalClasses;
      int maxAttendance;

      student = new Student { Id = 1 };

      maxRaw = 1000.0;
      totalClasses = 30;
      maxAttendance = 20;
      rawScore = 500.0;
      attendedClasses = 15;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedClasses);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      _ = calculator.CalculateCurrentScore(student, course);

      // DeepSeek: проверяем, что каждый репозиторий был вызван ровно один раз
      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }
  }
}
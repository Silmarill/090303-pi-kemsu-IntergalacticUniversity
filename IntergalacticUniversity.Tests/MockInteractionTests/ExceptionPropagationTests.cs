using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_PropagatesException() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;
      double maxRaw;
      int totalClasses;
      int maxAttendance;

      student = new Student { Id = 1 };

      maxRaw = 1000.0;
      totalClasses = 30;
      maxAttendance = 20;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course))
                    .Throws(new TimeoutException("Database timeout"));

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(student, course));
    }
  }
}
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendanceScoringTests {
    [TestCase(30, 10)]
    [TestCase(15, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendance_ReturnsExpectedAttendanceScore(int attended, double expectedAttendanceScore) {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 10
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1000);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(70.0 + expectedAttendanceScore));
    }
  }
}

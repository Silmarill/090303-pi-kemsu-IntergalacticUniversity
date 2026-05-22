using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class TestCaseExample {
    [TestCase(0, 0, 0, 0)]
    [TestCase(500, 15, 20, 10)]
    [TestCase(1000, 30, 40, 20)]
    public void CalculateCurrentScore_VariousInputs_ReturnsExpected(
        double rawScore, int attended, double expectedAssignments, double expectedAttendance) {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double result = calculator.CalculateCurrentScore(student, course);

      // Assert
      double expectedTotal = expectedAssignments + expectedAttendance;
      Assert.That(result, Is.EqualTo(expectedTotal));
    }
  }
}

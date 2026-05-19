using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_3_CapLimitTests {
    [Test]
    public void CalculateCurrentScore_WhenSumExceedsMax_LimitsToMax() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1200);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double current = calculator.CalculateCurrentScore(student, course);

      // Assert
      Assert.That(current, Is.EqualTo(80.0));
    }
  }
}
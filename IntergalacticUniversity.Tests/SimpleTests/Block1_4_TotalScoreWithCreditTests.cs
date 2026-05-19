using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_4_TotalScoreWithCreditTests {
    [Test]
    public void CalculateTotalScore_ForCreditCourse_AddsCreditCorrectly() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(92.8571428571);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(student, course, examOrCreditScore: 20);

      // Assert
      Assert.That(total, Is.EqualTo(95.0).Within(0.001));
    }
  }
}
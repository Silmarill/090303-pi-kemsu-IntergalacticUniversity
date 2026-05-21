using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesExactlyOnceWithCorrectArgs() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;

      student = new Student { Id = 1, Name = "Test" };
      course = new Course {
        CourseId = 1,
        Name = "Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 10
      };
      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(50);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(5);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateCurrentScore(student, course);

      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
    }
  }
}
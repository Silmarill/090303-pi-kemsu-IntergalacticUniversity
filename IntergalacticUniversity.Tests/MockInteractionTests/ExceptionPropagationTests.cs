using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

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

      student = new Student { Id = 1, Name = "Test" };
      course = new Course {
        CourseId = 1,
        Name = "Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 10
      };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Throws<TimeoutException>();

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(() => calculator.CalculateCurrentScore(student, course), Throws.TypeOf<TimeoutException>());
    }
  }
}
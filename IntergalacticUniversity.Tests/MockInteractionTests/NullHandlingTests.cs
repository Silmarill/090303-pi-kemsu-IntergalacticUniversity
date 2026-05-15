using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Core.Interfaces;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_UsesZeroForAssignments() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;
      double result;

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

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(10);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_UsesZeroForAttendance() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;
      double result;

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

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(100);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(50.0));
    }
  }
}
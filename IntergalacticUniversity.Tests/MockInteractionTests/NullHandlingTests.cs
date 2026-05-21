// ДипСик помог настроить мок на выброс исключения и проверить, что исключение пробрасывается дальше
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
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
      double expectedResult;

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
      expectedResult = 10.0;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(10);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(expectedResult).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_UsesZeroForAttendance() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;
      double result;
      double expectedResult;

      student = new Student { Id = 1, Name = "Test" };
      course = new Course {
        CourseId = 1,
        Name = "Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 10,
      };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      expectedResult = 50.0;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(100);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(expectedResult).Within(0.001));
    }
  }
}
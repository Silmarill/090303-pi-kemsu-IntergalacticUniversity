using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class SingleCallVerificationTests {
    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      double rawScore;
      int attendedClasses;
      double examScore;
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

      rawScore = 50.0;
      attendedClasses = 5;
      examScore = 30.0;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedClasses);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(student, course, examScore);

      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
    }
  }
}
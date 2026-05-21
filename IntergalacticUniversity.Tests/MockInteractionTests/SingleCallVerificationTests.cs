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
      RatingCalculator calculator;
      double maxRaw;
      int totalClasses;
      int maxAttendance;
      double rawScore;
      int attendedClasses;
      double examScore;

      student = new Student { Id = 1 };

      maxRaw = 1000.0;
      totalClasses = 30;
      maxAttendance = 20;
      rawScore = 500.0;
      attendedClasses = 15;
      examScore = 30.0;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedClasses);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: examScore);

      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }
  }
}
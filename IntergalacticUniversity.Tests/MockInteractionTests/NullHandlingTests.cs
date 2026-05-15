using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Student student;
    private Course course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;

    [SetUp]
    public void Setup() {
      student = new Student { Id = 1 };
      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_OnlyAttendanceContributes() {
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(student, course);
      Assert.That(current, Is.EqualTo(20.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_OnlyAssignmentsContribute() {
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1000.0);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(student, course);
      Assert.That(current, Is.EqualTo(40.0));
    }
  }
}
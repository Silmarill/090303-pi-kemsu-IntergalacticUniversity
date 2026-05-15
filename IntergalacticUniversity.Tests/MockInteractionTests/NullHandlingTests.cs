using Moq;
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

      double maxRaw = 1000.0;
      int totalClasses = 30;
      int maxAttendance = 20;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_OnlyAttendanceContributes() {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent = 20.0;
      int fullAttendance = course.TotalClasses;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(fullAttendance);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(student, course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_OnlyAssignmentsContribute() {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent = 40.0;
      double fullRawScore = course.MaxRawAssignmentsScore;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(fullRawScore);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(student, course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }
  }
}
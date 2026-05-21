using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void Setup() {
      _student = new Student { Id = 1 };

      double maxRaw = 1000.0;
      int totalClasses = 30;
      int maxAttendance = 20;

      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_OnlyAttendanceContributes() {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent = 20.0;
      int fullAttendance = _course.TotalClasses;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(fullAttendance);

      calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_OnlyAssignmentsContribute() {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent = 40.0;
      double fullRawScore = _course.MaxRawAssignmentsScore;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(fullRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }
  }
}
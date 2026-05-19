using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Student _student = null!;
    private Course _course = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15,
      };
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsAttendanceOnly() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(10);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(7.5));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_ReturnsAssignmentsOnly() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(300.0);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(22.5));
    }
  }
}

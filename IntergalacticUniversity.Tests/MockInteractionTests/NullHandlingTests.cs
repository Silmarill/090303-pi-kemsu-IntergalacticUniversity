using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_CountsOnlyAttendance() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(30);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(20.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_CountsOnlyAssignments() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(40.0));
    }
  }
}
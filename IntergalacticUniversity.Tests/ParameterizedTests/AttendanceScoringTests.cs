using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class AttendanceScoringTests {
    private Student? _student;
    private Course? _course;
    private Mock<IAttendanceRepository>? _mockAttendance;
    private Mock<IAssignmentsRepository>? _mockAssignments;
    private RatingCalculator? _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student {
        Id = 1,
        Name = "Тестовый Студент"
      };

      _course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _student = null;
      _course = null;
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_WithVariousAttendance_ReturnsExpectedAttendanceScore(int attended, double expectedAttendancePart) {
      double fullRawScore;
      fullRawScore = 1000;

      double maxAssignmentsScore;
      maxAssignmentsScore = 70;

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, _course!))
          .Returns(attended);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, _course!))
          .Returns(fullRawScore);

      double result;
      result = _calculator!.CalculateCurrentScore(_student!, _course!);

      double expectedTotal;
      expectedTotal = maxAssignmentsScore + expectedAttendancePart;
      Assert.That(result, Is.EqualTo(expectedTotal));
    }
  }
}
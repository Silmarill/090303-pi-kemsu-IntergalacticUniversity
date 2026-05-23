using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
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
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [TearDown]
    public void TearDown() {
      _student = null;
      _course = null;
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      int fullAttendance;
      fullAttendance = 40;

      double expectedAttendanceOnlyScore;
      expectedAttendanceOnlyScore = 20.0;

      double tolerance;
      tolerance = 0.001;

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, _course!))
          .Returns(fullAttendance);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, _course!))
          .Returns(null as double?);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result;
      result = _calculator.CalculateCurrentScore(_student!, _course!);

      Assert.That(result, Is.EqualTo(expectedAttendanceOnlyScore).Within(tolerance));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_ReturnsOnlyAssignmentsScore() {
      double fullRawScore;
      fullRawScore = 800;

      double expectedAssignmentsOnlyScore;
      expectedAssignmentsOnlyScore = 32.0;

      double tolerance;
      tolerance = 0.001;

      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, _course!))
          .Returns(null as int?);

      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, _course!))
          .Returns(fullRawScore);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result;
      result = _calculator.CalculateCurrentScore(_student!, _course!);

      Assert.That(result, Is.EqualTo(expectedAssignmentsOnlyScore).Within(tolerance));
    }
  }
}
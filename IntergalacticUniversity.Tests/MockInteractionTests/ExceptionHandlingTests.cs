using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class ExceptionHandlingTests {
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
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      _ = _mockAttendance!
          .Setup(r => r.GetAttendedClasses(_student!, _course!))
          .Throws<TimeoutException>();

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments!.Object);

      _ = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student!, _course!));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_PropagatesException() {
      _ = _mockAssignments!
          .Setup(r => r.GetRawScore(_student!, _course!))
          .Throws<TimeoutException>();

      _calculator = new RatingCalculator(_mockAttendance!.Object, _mockAssignments.Object);

      _ = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student!, _course!));
    }
  }
}
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class AdditionalRequirementsTests {
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;
    private Student _student;

    [SetUp]
    public void Setup() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      _student = new Student { Id = 99, Name = "The Test Student" };
    }

    // null/null for both repositories
    [Test]
    public void CalculateCurrentScore_BothReposReturnNull_ReturnsZero() {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(0.0));
    }

    // Credit cap
    [Test]
    public void CalculateTotalScore_CreditExceedsMax_CapsAt20() {
      Course course = CreateCourse(ExamType.Credit, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(50);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(5);

      double result = _calculator.CalculateTotalScore(_student, course, 50);

      Assert.That(result, Is.EqualTo(60.0));
    }

    // Scoring with the current 75 and a total of 95 + A developing complication
    [Test]
    public void CalculateTotalScore_CreditWithCurrent75_Returns95() {
      Course course = CreateCourse(ExamType.Credit, 100, 10, 20);

      int maxCurrent = 80;
      int expectedAttendance = 20;
      int maxAssignments = maxCurrent - expectedAttendance;

      double expectedAssignmentsScore = 55.0;

      double requiredRawPercent = expectedAssignmentsScore / maxAssignments;
      double requiredRawScore = course.MaxRawAssignmentsScore * requiredRawPercent;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(requiredRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(course.TotalClasses);

      double result = _calculator.CalculateTotalScore(_student, course, 20);

      Assert.That(result, Is.EqualTo(95.0).Within(0.001));
    }

    // Parameterization of tasks
    [TestCase(0, 0)]
    [TestCase(50, 20)]
    [TestCase(100, 40)]
    public void CalculateCurrentScore_AssignmentsOnly_VariousRawScores(double rawScore, double expected) {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(0);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expected));
    }

    // Parameterization of attendance
    [TestCase(0, 0)]
    [TestCase(5, 10)]
    [TestCase(10, 20)]
    public void CalculateCurrentScore_AttendanceOnly_VariousAttended(int attended, double expected) {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(0);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expected));
    }

    // Combined rawPercent/attendancePercent
    [TestCase(0, 0, 0)]
    [TestCase(25, 5, 20)]
    [TestCase(100, 10, 60)]
    public void CalculateCurrentScore_Combined_VariousInputs(double rawScore, int attended, double expected) {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expected));
    }

    // null for each repository
    [TestCase(null, 10, 20)]
    [TestCase(100, null, 40)]
    public void CalculateCurrentScore_OneRepoReturnsNull_CalculatesOtherCorrectly(double? rawScore, int? attended, double expected) {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(expected));
    }

    // Times.Once for CalculateTotalScore
    [Test]
    public void CalculateTotalScore_CallsRepositoriesExactlyOnce() {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(50);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(5);
      _ = _calculator.CalculateTotalScore(_student, course, 20);

      _mockAssignments.Verify(r => r.GetRawScore(_student, course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, course), Times.Once);
    }

    // TimeoutException via Throws
    [Test]
    public void CalculateCurrentScore_RepositoryThrowsTimeout_ThrowsTimeoutException() {
      Course course = CreateCourse(ExamType.Exam, 100, 10, 20);

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Throws<TimeoutException>();
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(10);

      // Throws returns the thrown exception, we discard it
      _ = Assert.Throws<TimeoutException>(() => _calculator.CalculateCurrentScore(_student, course));

      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, course), Times.Never);
    }

    // Auxiliary method
    private static Course CreateCourse(ExamType type, double maxRaw, int totalClasses, int maxAttendance) {
      return new Course {
        Type = type,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };
    }
  }
}
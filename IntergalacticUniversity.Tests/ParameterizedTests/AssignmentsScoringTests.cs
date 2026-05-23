using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsScoringTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

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
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(500, 20)]
    [TestCase(800, 32)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_WithVariousRawScores_ReturnsExpectedAssignmentsScore(double rawScore, double expectedAssignmentsPart) {
      int fullAttendance;
      fullAttendance = 40;

      int maxAttendanceScore;
      maxAttendanceScore = 20;

      double tolerance;
      tolerance = 0.001;

      _ = _mockAttendance
          .Setup(r => r.GetAttendedClasses(_student, _course))
          .Returns(fullAttendance);

      _ = _mockAssignments
          .Setup(r => r.GetRawScore(_student, _course))
          .Returns(rawScore);

      double result;
      result = _calculator.CalculateCurrentScore(_student, _course);

      double expectedTotal;
      expectedTotal = expectedAssignmentsPart + maxAttendanceScore;
      Assert.That(result, Is.EqualTo(expectedTotal).Within(tolerance));
    }
  }
}
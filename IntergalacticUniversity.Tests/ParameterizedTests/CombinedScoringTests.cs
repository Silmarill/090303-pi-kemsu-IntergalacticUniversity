using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScoringTests {
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
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
    }

    [TestCase(0, 0, 0)]
    [TestCase(300, 10, 30)]
    [TestCase(600, 20, 60)]
    public void CalculateCurrentScore_WithCombinedParameters_ReturnsExpectedCurrentScore(double rawScore, int attended, double expectedCurrent) {
      double tolerance;
      tolerance = 0.001;

      _ = _mockAttendance
          .Setup(r => r.GetAttendedClasses(_student, _course))
          .Returns(attended);

      _ = _mockAssignments
          .Setup(r => r.GetRawScore(_student, _course))
          .Returns(rawScore);

      double result;
      result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(expectedCurrent).Within(tolerance));
    }
  }
}
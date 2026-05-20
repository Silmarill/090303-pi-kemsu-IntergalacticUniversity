using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_4_CombinedScenariosTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      _mockAttendance = new Mock<IAttendanceRepository>();

      mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, mockAssignments.Object);
    }

    [TestCase(20, 30, 13.5)]
    [TestCase(50, 100, 37.5)]
    [TestCase(100, 20, 48)]
    [TestCase(100, 100, 60)]
    public void CalculateCurrentScore_CombinedScenarios_ReturnsCorrectScore(
            double rawPercent, double attendancePercent, double expectedCurrent) {
      double rawScore = (rawPercent / 100) * _course.MaxRawAssignmentsScore;
      int attended = (int)((attendancePercent / 100) * _course.TotalClasses);

      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
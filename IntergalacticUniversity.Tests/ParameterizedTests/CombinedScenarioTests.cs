using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenarioTests {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test Student" };

      _examCourse = new Course {
        CourseId = 101,
        Name = "Exam Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0, 0)]
    [TestCase(300, 10, 30)]
    [TestCase(600, 20, 60)]
    [TestCase(150, 5, 15)]
    public void CalculateCurrentScore_CombinedScenarios_ReturnsExpectedCurrentScore(
        double rawScore, int attendedClasses, double expectedCurrentScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(attendedClasses);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedCurrentScore));
    }
  }
}
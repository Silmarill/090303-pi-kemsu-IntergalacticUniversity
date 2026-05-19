using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
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
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      double expectedTotal = expectedAssignmentsScore + 20.0;
      Assert.That(result, Is.EqualTo(expectedTotal));
    }
  }
}
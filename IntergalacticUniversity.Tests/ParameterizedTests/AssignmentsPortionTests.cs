using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Core.Interfaces;
using Moq;

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
      int attendedClasses;

      _student = new Student { Id = 1, Name = "Test" };
      _examCourse = new Course {
        CourseId = 1,
        Name = "Exam",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      attendedClasses = 30;
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(attendedClasses);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_WithDifferentRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsPart) {
      double result;
      double attendancePart;
      double expected;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);

      result = _calculator.CalculateCurrentScore(_student, _examCourse);
      attendancePart = 20.0;
      expected = expectedAssignmentsPart + attendancePart;

      Assert.That(result, Is.EqualTo(expected));
    }
  }
}
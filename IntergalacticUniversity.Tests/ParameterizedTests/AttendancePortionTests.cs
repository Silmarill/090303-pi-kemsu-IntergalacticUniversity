using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Core.Interfaces;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTests {
    private Student _student;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      double rawScore;

      _student = new Student { Id = 1, Name = "Test" };
      _creditCourse = new Course {
        CourseId = 2,
        Name = "Credit",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      rawScore = 1000;
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(rawScore);
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_WithDifferentAttendance_ReturnsCorrectAttendancePart(int attended, double expectedAttendancePart) {
      double result;
      double maxCurrent;
      double maxAttendance;
      double assignmentsPart;
      double expected;

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attended);

      result = _calculator.CalculateCurrentScore(_student, _creditCourse);
      maxCurrent = 80;
      maxAttendance = 10;
      assignmentsPart = maxCurrent - maxAttendance;
      expected = assignmentsPart + expectedAttendancePart;

      Assert.That(result, Is.EqualTo(expected));
    }
  }
}
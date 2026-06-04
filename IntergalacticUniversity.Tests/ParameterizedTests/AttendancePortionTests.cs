using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
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
      _student = new Student { Id = 1, Name = "Test Student" };

      _creditCourse = new Course {
        CourseId = 101,
        Name = "Credit Course",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePortion(int attendedClasses, double expectedAttendanceScore) {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attendedClasses);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      double expectedTotal = 70.0 + expectedAttendanceScore;
      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}
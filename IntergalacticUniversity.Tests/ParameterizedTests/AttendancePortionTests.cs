using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTests {
    private Student student;
    private Course course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;

    [SetUp]
    public void Setup() {
      int studentId = 1;
      student = new Student { Id = studentId };

      double maxRawAssignmentsScore = 1000.0;
      int totalClasses = 20;
      int maxAttendanceScore = 10;

      course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      double fullRawScore = maxRawAssignmentsScore;
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(fullRawScore);
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendancePart) {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent;
      double maxCurrentCredit = 80.0;
      int maxAttendance = course.MaxAttendanceScore;
      double maxAssignmentsScore = maxCurrentCredit - maxAttendance;

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(student, course);
      expectedCurrent = maxAssignmentsScore + expectedAttendancePart;

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }
  }
}
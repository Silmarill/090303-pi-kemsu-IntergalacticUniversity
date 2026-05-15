using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenarioTests {
    private Student student;
    private Course course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;

    [SetUp]
    public void Setup() {
      int defaultStudentId = 1;
      student = new Student { Id = defaultStudentId };

      double maxRaw = 600.0;
      int totalClasses = 20;
      int maxAttendance = 15;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [TestCase(0.0, 0.0, 0)]
    [TestCase(0.5, 0.5, 30)]
    [TestCase(1.0, 0.0, 45)]
    [TestCase(0.0, 1.0, 15)]
    [TestCase(1.0, 1.0, 60)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        double rawPercent, double attendancePercent, double expectedCurrent) {
      double delta = 0.001;
      double rawScore = rawPercent * course.MaxRawAssignmentsScore;
      int attended = (int)(attendancePercent * course.TotalClasses);

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      double actualCurrent = calculator.CalculateCurrentScore(student, course);

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent).Within(delta));
    }
  }
}
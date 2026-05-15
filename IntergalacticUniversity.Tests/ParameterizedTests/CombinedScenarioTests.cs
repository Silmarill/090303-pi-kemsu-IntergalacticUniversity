using Moq;
using NUnit.Framework;
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
      student = new Student { Id = 1 };
      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600.0,
        TotalClasses = 20,
        MaxAttendanceScore = 15
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
      double rawScore = rawPercent * course.MaxRawAssignmentsScore;
      int attended = (int)(attendancePercent * course.TotalClasses);
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(student, course);
      Assert.That(current, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
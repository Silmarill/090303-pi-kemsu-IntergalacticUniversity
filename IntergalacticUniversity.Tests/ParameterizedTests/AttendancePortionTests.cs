using Moq;
using NUnit.Framework;
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
      student = new Student { Id = 1 };
      course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1000.0);
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendancePart) {
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(student, course);
      double expectedTotal = 70.0 + expectedAttendancePart;
      Assert.That(current, Is.EqualTo(expectedTotal));
    }
  }
}
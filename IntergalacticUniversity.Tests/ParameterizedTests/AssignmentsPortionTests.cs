using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student student;
    private Course course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;

    [SetUp]
    public void Setup() {
      student = new Student { Id = 1 };

      double maxRawAssignmentsScore = 1800.0;
      int totalClasses = 30;
      int maxAttendanceScore = 20;

      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();

      int attendedFull = totalClasses;
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attendedFull);
    }

    [TestCase(0, 0)]
    [TestCase(540, 12)]
    [TestCase(1800, 40)]
    public void CalculateCurrentScore_DifferentRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsPart) {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent;
      double attendancePart = 20.0;

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(student, course);
      expectedCurrent = expectedAssignmentsPart + attendancePart;

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }
  }
}
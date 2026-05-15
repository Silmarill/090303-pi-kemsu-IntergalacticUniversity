using Moq;
using NUnit.Framework;
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
      course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1800.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
    }

    [TestCase(0, 0)]
    [TestCase(540, 12)]
    [TestCase(1800, 40)]
    public void CalculateCurrentScore_DifferentRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsPart) {
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(student, course);
      double expectedTotal = expectedAssignmentsPart + 20.0;
      Assert.That(current, Is.EqualTo(expectedTotal));
    }
  }
}
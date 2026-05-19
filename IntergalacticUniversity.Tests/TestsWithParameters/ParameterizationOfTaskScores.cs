//Почти всё сделано ИИ

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.TestsWithParameters {
  public class ParameterizationOfTaskScores {
    private RatingCalculator _calculator;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };

      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(_course.TotalClasses);

      _mockAssignments = new Mock<IAssignmentsRepository>();

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void TaskScore(int rawScore, int expectedAssignmentsScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      double actualAssignmentsScore = current - 20;

      Assert.That(actualAssignmentsScore, Is.EqualTo(expectedAssignmentsScore));
    }
  }
}

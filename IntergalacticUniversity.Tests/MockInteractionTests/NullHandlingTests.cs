using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Mock<IAttendanceRepository> _attendanceMock;
    private Mock<IAssignmentsRepository> _assignmentsMock;
    private RatingCalculator _calculator;
    private Student _testStudent;
    private Course _testCourse;

    [SetUp]
    public void SetUp() {
      _attendanceMock = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _assignmentsMock = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_attendanceMock.Object, _assignmentsMock.Object);

      _testStudent = new Student { Id = 1 };
      _testCourse = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 20
      };
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, _testCourse)).Returns((double?)null);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, _testCourse)).Returns(10);

      double score = _calculator.CalculateCurrentScore(_testStudent, _testCourse);

      Assert.That(score, Is.EqualTo(10.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_ReturnsOnlyAssignmentsScore() {
      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, _testCourse)).Returns(500.0);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, _testCourse)).Returns((int?)null);

      double score = _calculator.CalculateCurrentScore(_testStudent, _testCourse);

      Assert.That(score, Is.EqualTo(20.0).Within(0.001));
    }
  }
}
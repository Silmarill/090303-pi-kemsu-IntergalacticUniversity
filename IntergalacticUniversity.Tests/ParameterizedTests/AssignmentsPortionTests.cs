using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Mock<IAttendanceRepository> _attendanceMock;
    private Mock<IAssignmentsRepository> _assignmentsMock;
    private RatingCalculator _calculator;
    private Student _testStudent;

    [SetUp]
    public void SetUp() {
      _attendanceMock = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _assignmentsMock = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_attendanceMock.Object, _assignmentsMock.Object);
      _testStudent = new Student { Id = 1, Name = "Тестовый Студент" };
    }

    [TestCase(0, 20.0)]
    [TestCase(300, 32.0)]
    [TestCase(1000, 60.0)]
    public void CalculateCurrentScore_VariousAssignmentsScores_ReturnsExpectedTotal(double rawScore, double expectedCurrent) {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns(rawScore);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns(30);

      double actual = _calculator.CalculateCurrentScore(_testStudent, course);

      Assert.That(actual, Is.EqualTo(expectedCurrent).Within(0.001));
    }

    [TestCase(20, 80.0)]
    [TestCase(10, 75.0)]
    [TestCase(0, 70.0)]
    public void CalculateCurrentScore_VariousAttendanceRecords_ReturnsExpectedTotal(int attended, double expectedCurrent) {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 500,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };

      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns(500.0);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns(attended);

      double actual = _calculator.CalculateCurrentScore(_testStudent, course);

      Assert.That(actual, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
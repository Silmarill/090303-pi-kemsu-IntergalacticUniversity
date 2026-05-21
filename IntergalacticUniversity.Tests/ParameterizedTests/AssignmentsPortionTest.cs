using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTest {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();

      // Экзамен: maxCurrent = 60, maxAttendance = 20 -> maxAssignments = 40
      _examCourse = TestDataFactory.CreateExamCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 20);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.2: параметризация приведения баллов за задания (посещаемость 100%)
    [TestCase(0, 0)] // 0% -> assignmentsScore = 0
    [TestCase(300, 12)] // 30% -> 0.3 * 40 = 12
    [TestCase(500, 20)] // 50% -> 20
    [TestCase(700, 28)] // 70% -> 28
    [TestCase(1000, 40)] // 100% -> 40

    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsScore) {
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns(40);

      double expectedAttendanceScore = 20; // 100% посещаемость
      double expectedTotal = expectedAssignmentsScore + expectedAttendanceScore;

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}

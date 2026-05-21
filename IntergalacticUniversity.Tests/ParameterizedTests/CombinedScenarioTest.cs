using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenarioTest {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();
      // Экзамен: maxCurrent = 60, maxAttendance = 15, maxAssignments = 45
      // MaxRawAssignmentsScore = 600, TotalClasses = 20
      _examCourse = TestDataFactory.CreateExamCourse(maxRawAssignmentsScore: 600, totalClasses: 20, maxAttendanceScore: 15);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.4: комбинированный параметризованный тест
    [TestCase(0.0, 0.0, 0)]           // 0% заданий, 0% посещений -> 0
    [TestCase(0.3, 0.5, 21)]          // 30% заданий = 0.3*45 = 13.5; 50% посещений = 0.5*15 = 7.5; сумма = 21
    [TestCase(0.7, 0.8, 43.5)]        // 70% заданий = 31.5; 80% посещений = 12; сумма = 43.5
    [TestCase(1.0, 1.0, 60)]          // 100% заданий и посещений -> 45+15 = 60

    public void CalculateCurrentScore_CombinedParameters_ReturnsExpectedCurrent(double rawPercent, double attendancePercent, double expectedCurrent) {
      double rawScore = rawPercent * _examCourse.MaxRawAssignmentsScore;
      int attended = (int)(attendancePercent * _examCourse.TotalClasses);

      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}

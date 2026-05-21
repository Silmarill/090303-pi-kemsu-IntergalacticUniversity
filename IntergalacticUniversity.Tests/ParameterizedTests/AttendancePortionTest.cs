using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTest {
    private Student _student;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();
      // Зачёт: maxCurrent = 80, maxAttendance = 10 -> maxAssignments = 70
      _creditCourse = TestDataFactory.CreateCreditCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 10);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 2.3: параметризация учёта посещаемости (задания 100%)
    [TestCase(40, 10)]      // 100% посещений -> attendanceScore = 10
    [TestCase(30, 7.5)]     // 75% -> 7.5
    [TestCase(20, 5)]       // 50% -> 5
    [TestCase(10, 2.5)]     // 25% -> 2.5
    [TestCase(0, 0)]        // 0% -> 0

    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendanceScore) {
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _creditCourse)).Returns(1000);
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _creditCourse)).Returns(attended);

      double expectedAssignmentsScore = 70; // 100% заданий
      double expectedTotal = expectedAssignmentsScore + expectedAttendanceScore;

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}

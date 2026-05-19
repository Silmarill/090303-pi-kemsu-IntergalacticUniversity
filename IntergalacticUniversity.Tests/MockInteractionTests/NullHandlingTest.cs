using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTest {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();
      _examCourse = TestDataFactory.CreateExamCourse(maxRawAssignmentsScore: 1000, totalClasses: 40, maxAttendanceScore: 20);
    }

    // Проверка 3.2: обработка null от репозитория (задания не сданы)
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns((double?)null);

      _mockAttendance = new Mock<IAttendanceRepository>();
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns(40);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      // Только баллы за посещаемость (20)
      Assert.That(result, Is.EqualTo(20.0));
    }

    // Проверка 3.2 (продолжение): обработка null от репозитория (нет данных о посещаемости)
    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_ReturnsOnlyAssignmentsScore() {
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _examCourse)).Returns(500); // 50%

      _mockAttendance = new Mock<IAttendanceRepository>();
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      // Только баллы за задания: 0.5*40 = 20
      Assert.That(result, Is.EqualTo(20.0));
    }
  }
}

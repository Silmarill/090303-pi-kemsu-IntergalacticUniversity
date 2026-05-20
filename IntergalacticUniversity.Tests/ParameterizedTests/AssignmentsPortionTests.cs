using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };

      // Exam: maxCurrent=60, maxAttendance=20, maxAssignments=40
      // Посещаемость фиксирована 100% => attendanceScore = 20
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      // Посещаемость 100% зафиксирована один раз в SetUp
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(30);
    }

    // Проверка 2.2 - разные проценты выполнения заданий при фиксированной посещаемости 100%
    // rawScore, ожидаемая часть за задания (attendanceScore всегда 20)
    [TestCase(0.0, 0.0)]          // 0% заданий => 0 + 20 = 20
    [TestCase(300.0, 12.0)]       // 30% от 1000 => 0.3*40=12 + 20 = 32
    [TestCase(1000.0, 40.0)]      // 100% => 40 + 20 = 60
    public void CalculateCurrentScore_VariousRawScores_ReturnsExpectedTotal(
        double rawScore, double expectedAssignmentsPart) {
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(rawScore);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _course);

      // Ожидаем: задания + посещаемость (20 при 100%)
      double expected = expectedAssignmentsPart + 20.0;
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}

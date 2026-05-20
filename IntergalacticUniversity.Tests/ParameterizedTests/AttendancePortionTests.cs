using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };

      // Credit: maxCurrent=80, maxAttendance=10, maxAssignments=70
      // Задания фиксированы 100% => assignmentsScore = 70
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 10,
        MaxAttendanceScore = 10
      };

      _mockAssignments = new Mock<IAssignmentsRepository>();
      // Задания 100% зафиксированы один раз в SetUp
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000.0);
    }

    // Проверка 2.3 - разные проценты посещаемости при фиксированных заданиях 100%
    // attended, ожидаемая часть за посещаемость
    [TestCase(10, 10.0)]   // 100% => attendanceScore=10, итого 70+10=80
    [TestCase(5, 5.0)]     // 50%  => attendanceScore=5,  итого 70+5=75
    [TestCase(0, 0.0)]     // 0%   => attendanceScore=0,  итого 70+0=70
    public void CalculateCurrentScore_VariousAttendance_ReturnsExpectedTotal(
        int attended, double expectedAttendancePart) {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, _mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _course);

      // Ожидаем: 70 (задания 100%) + часть за посещаемость
      double expected = 70.0 + expectedAttendancePart;
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}

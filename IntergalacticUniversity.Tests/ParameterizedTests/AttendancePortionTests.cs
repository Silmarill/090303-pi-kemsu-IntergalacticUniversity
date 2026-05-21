using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AttendancePortionTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void Setup() {
      int studentId = 1;
      _student = new Student { Id = studentId };

      double maxRawAssignmentsScore = 1000.0;
      int totalClasses = 20;
      int maxAttendanceScore = 10;

      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();

      // DeepSeek подсказал вынести настройку полного rawScore в Setup
      double fullRawScore = maxRawAssignmentsScore;
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(fullRawScore);
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendancePart) {
      RatingCalculator calculator;
      double actualCurrent;
      double expectedCurrent;
      double maxCurrentCredit = 80.0;
      int maxAttendance = _course.MaxAttendanceScore;
      double maxAssignmentsScore = maxCurrentCredit - maxAttendance;

      // DeepSeek подсказал использовать Returns с вычисляемым значением
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(attended);

      calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
      actualCurrent = calculator.CalculateCurrentScore(_student, _course);
      expectedCurrent = maxAssignmentsScore + expectedAttendancePart;

      Assert.That(actualCurrent, Is.EqualTo(expectedCurrent));
    }
  }
}
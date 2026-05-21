using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenarioTests {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    // ДипСик помог объединить в одном параметризованном тесте и проценты заданий, и проценты посещаемости
    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test" };
      _examCourse = new Course {
        CourseId = 3,
        Name = "Exam Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // ДипСик подсказал использовать TestCase с разными комбинациями данных для проверки граничных значений
    [TestCase(0, 0, 0)]
    [TestCase(300, 10, 30)]
    [TestCase(600, 20, 60)]
    [TestCase(150, 5, 15)]
    public void CalculateCurrentScore_CombinedParameters_ReturnsExpectedCurrent(double rawScore, int attended, double expectedCurrent) {
      double result;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(attended);

      result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
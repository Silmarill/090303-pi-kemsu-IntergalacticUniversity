using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _examCourse = new Course {
        CourseId = 1,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      _creditCourse = new Course {
        CourseId = 2,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_VariousRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(30);

      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedAssignmentsScore + 20));
    }

    [TestCase(20, 10)]
    [TestCase(10, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_VariousAttendance_ReturnsCorrectAttendancePortion(int attended, double expectedAttendanceScore) {
      _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000);
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(70 + expectedAttendanceScore));
    }
  }
}
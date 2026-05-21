// ДипСик помог реализовать параметризованные тесты с TestCase для проверки разных процентов выполнения заданий
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      int attendedClasses;
      double maxRawAssignmentsScore;
      int totalClasses;
      int maxAttendanceScore;
      int courseId;
      string courseName;
      ExamType examType;

      attendedClasses = 30;
      maxRawAssignmentsScore = 1000.0;
      totalClasses = 30;
      maxAttendanceScore = 20;
      courseId = 1;
      courseName = "Exam";
      examType = ExamType.Exam;

      _student = new Student { Id = 1, Name = "Test" };
      _examCourse = new Course {
        CourseId = courseId,
        Name = courseName,
        Type = examType,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(attendedClasses);
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_WithDifferentRawScores_ReturnsCorrectAssignmentsPortion(double rawScore, double expectedAssignmentsPart) {
      double attendancePart;
      double expectedCurrentScore;
      double actualCurrentScore;

      attendancePart = 20.0;
      expectedCurrentScore = expectedAssignmentsPart + attendancePart;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);

      actualCurrentScore = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(actualCurrentScore, Is.EqualTo(expectedCurrentScore).Within(0.001));
    }
  }
}
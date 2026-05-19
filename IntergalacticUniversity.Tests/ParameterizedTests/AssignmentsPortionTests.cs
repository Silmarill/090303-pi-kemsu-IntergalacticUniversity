using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class AssignmentsPortionTests {
    private Student _student = null!;
    private Course _examCourse = null!;
    private Course _creditCourse = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _examCourse = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20,
      };
      _creditCourse = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 10,
        MaxAttendanceScore = 10,
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(30);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000.0);
    }

    [TestCase(0, 20)]
    [TestCase(300, 32)]
    [TestCase(1000, 60)]
    public void CalculateCurrentScore_AssignmentsPercentWithFullAttendance_ReturnsExpected(
        double rawScore,
        double expectedCurrent) {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);

      double current = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(current, Is.EqualTo(expectedCurrent));
    }

    [TestCase(10, 80)]
    [TestCase(5, 75)]
    [TestCase(0, 70)]
    public void CalculateCurrentScore_AttendancePercentWithFullAssignments_ReturnsExpected(
        int attended,
        double expectedCurrent) {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attended);

      double current = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(current, Is.EqualTo(expectedCurrent));
    }
  }
}

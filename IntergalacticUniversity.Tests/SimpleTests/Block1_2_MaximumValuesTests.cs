using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_2_MaximumValuesTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _student = null;
      _course = null;
      _mockAttendance = null;
      _mockAssignments = null;
      _calculator = null;
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMax() {
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(800);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(60.0));

      string grade = _calculator.ConvertToGrade(100);
      Assert.That(grade, Is.EqualTo("Отлично"));
    }
  }
}
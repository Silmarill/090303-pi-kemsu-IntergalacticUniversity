using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_4_TotalScoreWithCreditTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 20,
        MaxAttendanceScore = 10
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
    public void CalculateTotalScore_ForCreditCourse_AddsCreditCorrectly() {
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(92.8571428571);

      double result = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 20);

      Assert.That(result, Is.EqualTo(95.0).Within(0.001));
    }
  }
}
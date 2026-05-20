using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_4_TotalScoreWithCreditTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
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

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _student = null!;
      _course = null!;
      mockAttendance = null!;
      mockAssignments = null!;
      _calculator = null!;
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_AddsCreditCorrectly() {
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(92.8571428571);

      double result = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 20);

      Assert.That(result, Is.EqualTo(95.0).Within(0.001).Within(0.001));
    }
  }
}
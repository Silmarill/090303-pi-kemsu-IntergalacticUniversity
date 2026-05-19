using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тест" };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAllMax_ReturnsMaxCurrent() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(800);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(60.0));
    }

    [Test]
    public void ConvertToGrade_WhenAllMax_ReturnsExcellent() {
      Assert.That(_calculator.ConvertToGrade(100), Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenOverMax_DoesNotExceedMaxCurrent() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1200);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.LessThanOrEqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_ReturnsSumCappedAt100() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1000);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(30);

      double result = _calculator.CalculateTotalScore(_student, course, examOrCreditScore: 20);

      Assert.That(result, Is.LessThanOrEqualTo(100.0));
    }
  }
}
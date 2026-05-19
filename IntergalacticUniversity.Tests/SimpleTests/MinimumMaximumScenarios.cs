using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student = null!;
    private Course _course = null!;
    private Mock<IAttendanceRepository> _mockAttendance = null!;
    private Mock<IAssignmentsRepository> _mockAssignments = null!;
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse();
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance = null!;
      _mockAssignments = null!;
      _calculator = null!;
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _course = TestDataFactory.CreateExamCourse();
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrent() {
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20,
      };
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(800.0);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);

      double current = _calculator.CalculateCurrentScore(_student, _course);
      string grade = _calculator.ConvertToGrade(100);

      Assert.That(current, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenExceedsMaximum_IsCappedAtMaxCurrent() {
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 15,
      };
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1200.0);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_WhenCreditWithMaxFinal_ReturnsExpectedTotal() {
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 5,
      };
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000.0);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(0);

      double total = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 20);

      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}

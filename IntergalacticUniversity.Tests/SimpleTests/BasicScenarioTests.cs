using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class BasicScenarioTests {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_NullAttendanceAndNullAssignments_ReturnsZero() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns((int?)null);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns((double?)null);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_FullAttendanceAndFullAssignments_ReturnsMaxCurrentForExam() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(800);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(40);

      double current = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(current, Is.EqualTo(60.0));

      string grade = _calculator.ConvertToGrade(100.0);
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_Overfulfillment_DoesNotExceedMaxCurrentForCredit() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1200);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(30);

      double result = _calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_CreditWithCurrentScore75AndCredit20_Returns95() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 13,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(12);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double total = calculator.CalculateTotalScore(student, course, 20.0);

      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}
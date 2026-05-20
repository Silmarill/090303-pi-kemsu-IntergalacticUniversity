// ИИ написал строку 49 и 52, так как я не до конца разобрался в использовании Тестов
// ИИ помогал написать SetUp, подсказал, что там должно быть
// ИИ подсказал, как добавить Math.Min в 72 строку

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleBlockTest {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void Setup() {
      _student = new Student { Id = 42 };
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

    [Test]
    public void NullTest() {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((int?)null);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(0));
    }

    [Test]
    public void MaxValues() {
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(800);

      double current = _calculator.CalculateCurrentScore(_student, _course);
      string grade = _calculator.ConvertToGrade(100);

      Assert.That(current, Is.EqualTo(60));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void ValueRange() {
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(30);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1200);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(Math.Min(current, 80)));
    }

    public void CorrectAddition() {
      _course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(30);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(1000);

      double current = _calculator.CalculateCurrentScore(_student, _course);
      double total = _calculator.CalculateTotalScore(_student, _course, 20);

      Assert.That(current, Is.EqualTo(75));
      Assert.That(total, Is.EqualTo(95));
      Assert.That(total, Is.EqualTo(Math.Min(total, 100)));
    }
  }
}
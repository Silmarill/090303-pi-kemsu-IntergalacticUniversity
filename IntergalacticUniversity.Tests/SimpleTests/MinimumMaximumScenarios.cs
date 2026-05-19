using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance = null;
      _mockAssignments = null;
    }

    // Проверка 1.1 - оба репозитория возвращают null => результат 0
    [Test]
    public void CalculateCurrentScore_WhenBothReposReturnNull_ReturnsZero() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns((int?)null);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.EqualTo(0.0));
    }

    // Проверка 1.2 - 100% заданий и 100% посещаемость (Exam) => текущая = 60, оценка "Отлично"
    [Test]
    public void CalculateCurrentScore_WhenExamAndFullMarks_ReturnsMaxCurrentAndExcellentGrade() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      // 800/800 * 40 = 40 (задания) + 40/40 * 20 = 20 (посещаемость) = 60
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(40);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(800.0);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double current = calculator.CalculateCurrentScore(_student, course);
      string grade = calculator.ConvertToGrade(100);

      Assert.That(current, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    // Проверка 1.3 - rawScore превышает MaxRaw (Credit) => результат не превышает 80 (Math.Min)
    [Test]
    public void CalculateCurrentScore_WhenCreditAndRawScoreExceedsMax_CapsAtMaxCurrent() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      // rawScore 1200 > 1000, Math.Min обрежет часть заданий до 65
      // 65 (задания) + 15 (посещаемость 100%) = 80 <= 80
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(20);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1200.0);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, course);

      Assert.That(result, Is.LessThanOrEqualTo(80.0));
    }

    // Проверка 1.4 - Credit, текущая 75, зачёт 20 => итого 95 (ограничение 100 не срабатывает)
    [Test]
    public void CalculateTotalScore_WhenCreditAndSumUnder100_ReturnsCorrectTotal() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 20
      };

      // maxCurrent=80, maxAssignments=60
      // 1000/1000 * 60 = 60 (задания) + 15/20 * 20 = 15 (посещаемость) = 75
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(15);
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(1000.0);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      double total = calculator.CalculateTotalScore(_student, course, examOrCreditScore: 20);

      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleTestExample {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };

      _examCourse = new Course {
        CourseId = 101,
        Name = "Экзаменационный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 80,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance.Reset();
      _mockAssignments.Reset();
    }

    // Проверка 1.1: Минимальные значения – всё на нуле
    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      // Assert
      Assert.That(result, Is.EqualTo(0.0));
    }

    // Проверка 1.2: Максимальные значения – 100% заданий и 100% посещаемости
    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrentForExam() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(80);

      // Act
      double currentScore = _calculator.CalculateCurrentScore(_student, _examCourse);
      string grade = _calculator.ConvertToGrade(100);

      // Assert
      Assert.That(currentScore, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    // Проверка 1.3: Проверка ограничения сверху – сумма не может превысить maxCurrent
    [Test]
    public void CalculateCurrentScore_WhenScoresExceedMaximum_ReturnsCappedValue() {
      // Arrange
      Course highScoreCourse = new Course {
        CourseId = 103,
        Name = "Курс с завышенными баллами",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, highScoreCourse)).Returns(30);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, highScoreCourse)).Returns(1200);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, highScoreCourse);

      // Assert
      Assert.That(result, Is.LessThanOrEqualTo(80.0));
      Assert.That(result, Is.EqualTo(80.0));
    }

    // Проверка 1.4: Итоговый балл с зачётом – правильное сложение
    [Test]
    public void CalculateTotalScore_ForCreditCourse_AddsCreditScoreCorrectly() {
      // Arrange
      // maxCurrent = 80, maxAttendance = 15 → maxAssignments = 65
      // Чтобы получить current = 75, нужно:
      // assignments: 100% = 65 баллов
      // attendance: 10 баллов из 15 (66.7% посещаемости = 20 занятий из 30)
      Course creditCourse = new Course {
        CourseId = 104,
        Name = "Зачётный курс",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      // 100% заданий = 65 баллов
      // 20 посещённых занятий из 30 = 66.7% = 10 баллов
      // Итого current = 75
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(20);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(100);

      // Act
      double totalScore = _calculator.CalculateTotalScore(_student, creditCourse, examOrCreditScore: 20);

      // Assert: 75 + 20 = 95
      Assert.That(totalScore, Is.EqualTo(95.0));
    }
  }
}
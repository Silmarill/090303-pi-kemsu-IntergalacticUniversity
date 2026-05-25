// MockExample.cs - Блок 3: Тесты с моками (4 проверки)
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class MockExample {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Тестовый Студент" };
      _course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Проверка 3.1: Проверка вызова методов с правильными аргументами
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      _ = _calculator.CalculateCurrentScore(_student, _course);

      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }

    // Проверка 3.2: Обработка null от репозитория
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_UsesZeroForAssignments() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      // assignments=0, attendance=20
      Assert.That(result, Is.EqualTo(20.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_UsesZeroForAttendance() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(100);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      // assignments=40, attendance=0
      Assert.That(result, Is.EqualTo(40.0).Within(0.001));
    }

    // Проверка 3.3: Отсутствие повторных вызовов репозиториев
    [Test]
    public void CalculateTotalScore_DoesNotCallRepositoriesTwice() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      _ = _calculator.CalculateTotalScore(_student, _course, 30);

      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }

    // Проверка 3.4: Симуляция исключений
    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrows_PropagatesException() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course))
          .Throws(new TimeoutException("Таймаут при подключении к БД"));

      _ = Assert.Throws<TimeoutException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrows_PropagatesException() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course))
          .Throws(new TimeoutException("Таймаут при чтении данных"));
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      _ = Assert.Throws<TimeoutException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }

    // Дополнительный тест: проверка суммы с ограничением экзаменационного балла
    [Test]
    public void CalculateTotalScore_ExamWithExamScore_ReturnsCorrectSum() {
      // rawScore=400 при MaxRaw=100 -> 100% -> assignmentsScore = 40
      // attendance=20/40=50% -> attendanceScore = 10
      // current = 50, examScore=30 -> total = 80
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(400);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      double total = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 30);

      Assert.That(total, Is.EqualTo(80.0));
    }
  }
}
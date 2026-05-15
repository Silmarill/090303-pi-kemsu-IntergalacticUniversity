// MockExample.cs - Блок 3: Тесты с моками (4 проверки)
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
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

    // Проверка 3.1: Проверка вызова методов репозитория с правильными аргументами
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult1 = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      // Act
      double vovchik = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }

    // Проверка 3.2: Обработка null от репозитория
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_UsesZeroForAssignments() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult1 = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(40);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert: assignments=0, attendance=20
      Assert.That(result, Is.EqualTo(20.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_UsesZeroForAttendance() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(100);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Assert: assignments=40, attendance=0
      Assert.That(result, Is.EqualTo(40.0).Within(0.001));
    }

    // Проверка 3.3: Проверка, что метод CalculateTotalScore не вызывает репозитории повторно
    [Test]
    public void CalculateTotalScore_CallsRepositoriesExactlyOnce_NotTwice() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      // Act
      double vovchik = _calculator.CalculateTotalScore(_student, _course, 30);

      // Assert - каждый репозиторий вызывается ровно один раз
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_WhenExamScoreNotProvided_StillCallsRepositoriesOnce() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      // Act
      double vovchik = _calculator.CalculateTotalScore(_student, _course);

      // Assert
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
    }

    // Проверка 3.4: Симуляция исключения при доступе к данным
    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrows_PropagatesException() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);
      Moq.Language.Flow.IThrowsResult throwsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course))
          .Throws(new TimeoutException("Таймаут при подключении к БД"));

      // Act & Assert
      TimeoutException? timeoutException = Assert.Throws<TimeoutException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrows_PropagatesException() {
      // Arrange
      Moq.Language.Flow.IThrowsResult throwsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course))
          .Throws(new InvalidOperationException("Ошибка чтения данных"));
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      // Act & Assert
      InvalidOperationException? invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }

    // Дополнительный тест: проверка ограничения баллов за экзамен/зачёт
    [Test]
    public void CalculateTotalScore_ExamWithExamScore_ReturnsCorrectSum() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(400);
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult1 = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);

      // Act
      double total = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 30);

      // Ожидаем: задания 0.5*(60-20)=20, посещаемость 0.5*20=10, экзамен 30 -> итого 60
      Assert.That(total, Is.EqualTo(60.0));
    }
  }
}
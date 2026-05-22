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
        CourseId = 101,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _mockAttendance.Reset();
      _mockAssignments.Reset();
    }

    // Существующий тест из примера
    [Test]
    public void CalculateTotalScore_ExamWithExamScore_ReturnsCorrectSum() {
      // Arrange
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, course)).Returns(20);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, course)).Returns(400);

      RatingCalculator calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(_student, course, examOrCreditScore: 30);

      // Assert
      Assert.That(total, Is.EqualTo(60.0));
    }

    // === БЛОК 3: ПРОДВИНУТЫЕ ТЕСТЫ С МОКАМИ ===

    // Проверка 3.1: Проверка вызова методов репозитория с правильными аргументами
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments_OnceEach() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);

      // Act
      _ = _calculator.CalculateCurrentScore(_student, _course);

      // Assert
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.2: Обработка null от репозитория
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_UsesZeroForAssignments() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Ожидаем: только баллы за посещаемость (20/40*20 = 10)
      Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_UsesZeroForAttendance() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);

      // Act
      double result = _calculator.CalculateCurrentScore(_student, _course);

      // Ожидаем: только баллы за задания (50/100*40 = 20)
      Assert.That(result, Is.EqualTo(20.0));
    }

    // Проверка 3.3: Проверка, что метод CalculateTotalScore не вызывает репозитории повторно
    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce_NotTwice() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult1 = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);

      // Act
      _ = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 30);

      // Assert
      _mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    // Проверка 3.4: Симуляция исключения при доступе к данным
    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      // Arrange
      Moq.Language.Flow.IThrowsResult throwsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course))
          .Throws<TimeoutException>();
      Moq.Language.Flow.IReturnsResult<IAssignmentsRepository> returnsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(50);

      // Act & Assert
      TimeoutException? timeoutException = Assert.Throws<TimeoutException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_PropagatesException() {
      // Arrange
      Moq.Language.Flow.IReturnsResult<IAttendanceRepository> returnsResult = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(20);
      Moq.Language.Flow.IThrowsResult throwsResult = _mockAssignments.Setup(r => r.GetRawScore(_student, _course))
          .Throws<InvalidOperationException>();

      // Act & Assert
      InvalidOperationException? invalidOperationException = Assert.Throws<InvalidOperationException>(() =>
          _calculator.CalculateCurrentScore(_student, _course));
    }
  }
}
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class MockExample {
    [Test]
    public void CalculateTotalScore_ExamWithExamScore_ReturnsCorrectSum() {
      // Arrange
      Student student = new Student { Id = 42 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      // Мокируем репозитории, чтобы вернуть конкретные значения
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(repo => repo.GetAttendedClasses(student, course)).Returns(20); // 50%

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(repo => repo.GetRawScore(student, course)).Returns(400);     // 50%

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);

      // Assert
      // Ожидаем: задания 0.5*(60-20)=20, посещаемость 0.5*20=10, экзамен 30 -> итого 60
      Assert.That(total, Is.EqualTo(60.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      // Arrange
      Student student = new Student { Id = 67 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(repo => repo.GetAttendedClasses(student, course)).Returns((int?)null);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(repo => repo.GetRawScore(student, course)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double result = calculator.CalculateCurrentScore(student, course);

      // Assert
      Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void CalculateCurrentScore_WithMaxRawScoreAndFullAttendance_Returns60AndGradeExcellent() {
      // Arrange
      Student student = new Student { Id = 67 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(repo => repo.GetAttendedClasses(student, course)).Returns(40);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(repo => repo.GetRawScore(student, course)).Returns(800);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double currentScore = calculator.CalculateCurrentScore(student, course);
      string grade = calculator.ConvertToGrade(100);

      // Assert
      Assert.That(currentScore, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_WhenScoresExceedMax_CapsAtMaxCurrent() {
      // Arrange
      Student student = new Student { Id = 67 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(repo => repo.GetAttendedClasses(student, course)).Returns(40);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(repo => repo.GetRawScore(student, course)).Returns(1200); // больше максимума

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double result = calculator.CalculateCurrentScore(student, course);

      // Assert
      Assert.That(result, Is.EqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_WithCreditAndExamScore_Returns95() {
      // Arrange
      Student student = new Student { Id = 67 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();

      // Настраиваем так, чтобы посещаемость дала 10 баллов
      _ = mockAttendance.Setup(repo => repo.GetAttendedClasses(student, course)).Returns(27);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(repo => repo.GetRawScore(student, course)).Returns(1000); // 65 баллов за задания

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(student, course, examOrCreditScore: 20);

      // Assert
      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}
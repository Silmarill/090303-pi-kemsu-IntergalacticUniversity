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
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20); // 50%

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(400);     // 50%

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);

      // Assert
      // Ожидаем: задания 0.5*(60-20)=20, посещаемость 0.5*20=10, экзамен 30 -> итого 60
      Assert.That(total, Is.EqualTo(60.0));
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      Mock<IAssignmentsRepository> assignmentsMock = new Mock<IAssignmentsRepository>();
      Mock<IAttendanceRepository> attendanceMock = new Mock<IAttendanceRepository>();
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        MaxAttendanceScore = 20,
        TotalClasses = 40
      };

      _ = assignmentsMock.Setup(r => r.GetRawScore(student, course)).Returns(600);
      _ = attendanceMock.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);

      RatingCalculator calculator = new RatingCalculator(attendanceMock.Object, assignmentsMock.Object);

      _ = calculator.CalculateTotalScore(student, course, 15);

      assignmentsMock.Verify(r => r.GetRawScore(student, course), Times.Once);
      attendanceMock.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_OnlyAssignmentsScoreUsed() {
      Mock<IAssignmentsRepository> assignmentsMock = new Mock<IAssignmentsRepository>();
      Mock<IAttendanceRepository> attendanceMock = new Mock<IAttendanceRepository>();
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        MaxAttendanceScore = 20,
        TotalClasses = 40
      };

      _ = assignmentsMock.Setup(r => r.GetRawScore(student, course)).Returns(800.0);
      _ = attendanceMock.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      RatingCalculator calculator = new RatingCalculator(attendanceMock.Object, assignmentsMock.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      // 100% заданий = 40 баллов (60 - 20 = 40)
      Assert.That(result, Is.EqualTo(40).Within(0.001));
    }

    // ===== ДОБАВЛЕННЫЙ ТЕСТ 2: Проверка вызова методов репозитория =====
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesExactlyOnce() {
      Mock<IAssignmentsRepository> assignmentsMock = new Mock<IAssignmentsRepository>();
      Mock<IAttendanceRepository> attendanceMock = new Mock<IAttendanceRepository>();
      Student student = new Student { Id = 1 };
      Course course = new Course { Type = ExamType.Exam };

      _ = assignmentsMock.Setup(r => r.GetRawScore(student, course)).Returns(500);
      _ = attendanceMock.Setup(r => r.GetAttendedClasses(student, course)).Returns(15);

      RatingCalculator calculator = new RatingCalculator(attendanceMock.Object, assignmentsMock.Object);

      _ = calculator.CalculateCurrentScore(student, course);

      assignmentsMock.Verify(r => r.GetRawScore(student, course), Times.Once);
      attendanceMock.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_OnlyAttendanceScoreUsed() {
      Mock<IAssignmentsRepository> assignmentsMock = new Mock<IAssignmentsRepository>();
      Mock<IAttendanceRepository> attendanceMock = new Mock<IAttendanceRepository>();
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        MaxAttendanceScore = 20,
        TotalClasses = 40
      };

      _ = assignmentsMock.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      _ = attendanceMock.Setup(r => r.GetAttendedClasses(student, course)).Returns(40);

      RatingCalculator calculator = new RatingCalculator(attendanceMock.Object, assignmentsMock.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_PropagatesException() {
      Mock<IAssignmentsRepository> assignmentsMock = new Mock<IAssignmentsRepository>();
      Mock<IAttendanceRepository> attendanceMock = new Mock<IAttendanceRepository>();
      Student student = new Student { Id = 1 };
      Course course = new Course { Type = ExamType.Exam };

      _ = assignmentsMock.Setup(r => r.GetRawScore(student, course))
          .Throws(new TimeoutException());

      RatingCalculator calculator = new RatingCalculator(attendanceMock.Object, assignmentsMock.Object);

      _ = Assert.Throws<TimeoutException>(() =>
          calculator.CalculateCurrentScore(student, course));
    }
  }
}
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
  }
}
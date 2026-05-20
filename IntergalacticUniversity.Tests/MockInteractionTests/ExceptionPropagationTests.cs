using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    // Проверка 3.3 - CalculateTotalScore вызывает репозитории ровно один раз
    // (т.к. внутри вызывает CalculateCurrentScore, который сам обращается к репозиториям)
    [Test]
    public void CalculateTotalScore_CallsRepositoriesExactlyOnceEach() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(400.0);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);

      // Каждый репозиторий должен быть вызван ровно один раз, не два
      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }

    // Проверка 3.4 - репозиторий выбрасывает исключение => RatingCalculator не глотает его
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrows_PropagatesException() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      // Симулируем сбой инфраструктуры (например, таймаут базы)
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course))
            .Throws<System.TimeoutException>();

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500.0);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Модуль не должен скрывать ошибки инфраструктуры
      Assert.Throws<System.TimeoutException>(() => calculator.CalculateCurrentScore(student, course));
    }
  }
}

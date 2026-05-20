using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    // Проверка 3.2а - GetRawScore возвращает null (студент не сдал задания)
    // Ожидаем: только баллы за посещаемость, без исключения
    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_ReturnsOnlyAttendanceScore() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      // 50% посещаемость => 0.5 * 20 = 10
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(10);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      // Ожидаем только посещаемость: 10
      Assert.That(result, Is.EqualTo(10.0));
    }

    // Проверка 3.2б - GetAttendedClasses возвращает null (нет данных о посещаемости)
    // Ожидаем: только баллы за задания, без исключения
    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_ReturnsOnlyAssignmentsScore() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      // 50% заданий => 0.5 * 40 = 20 (maxAssignments = 60-20 = 40)
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500.0);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      // Ожидаем только задания: 20
      Assert.That(result, Is.EqualTo(20.0));
    }
  }
}

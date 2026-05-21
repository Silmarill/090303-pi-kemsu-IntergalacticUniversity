using IntergalacticUniversity.Tests.Common;
using IntergalacticUniversity.Core.Interfaces;
using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTest {
    // Проверка 3.4: симуляция исключения при доступе к данным
    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      Student student = TestDataFactory.CreateTestStudent();
      Course course = TestDataFactory.CreateExamCourse();

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course))
        .Throws(new TimeoutException("Ошибка подключения к базе данных посещаемости"));
      _ = mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(
        () => calculator.CalculateCurrentScore(student, course),
        Throws.TypeOf<TimeoutException>().With.Message.EqualTo("Ошибка подключения к базе данных посещаемости"));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_PropagatesException() {
      Student student = TestDataFactory.CreateTestStudent();
      Course course = TestDataFactory.CreateExamCourse();

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course)).Returns(20);
      _ = mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(student, course))
        .Throws(new InvalidOperationException("Некорректные данные в репозитории заданий"));

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(
        () => calculator.CalculateCurrentScore(student, course),
        Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Некорректные данные в репозитории заданий"));
    }

    [Test]
    public void CalculateTotalScore_WhenRepositoryThrowsException_PropagatesException() {
      Student student = TestDataFactory.CreateTestStudent();
      Course course = TestDataFactory.CreateExamCourse();

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course)).Returns(20);
      _ = mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(student, course))
        .Throws(new Exception("Непредвиденная ошибка доступа к данным"));

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(
        () => calculator.CalculateTotalScore(student, course, examOrCreditScore: 30),
        Throws.TypeOf<Exception>().With.Message.EqualTo("Непредвиденная ошибка доступа к данным"));
    }
  }
}

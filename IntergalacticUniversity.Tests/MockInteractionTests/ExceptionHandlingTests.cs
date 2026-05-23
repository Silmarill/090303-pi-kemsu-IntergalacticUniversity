using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionHandlingTests {
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _student = new Student {
        Id = 1,
        Name = "Тестовый Студент"
      };

      _course = new Course {
        CourseId = 1,
        Name = "Тестовый курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
    }

    [TearDown]
    public void TearDown() {
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_PropagatesException() {
      double fullRawScore;
      fullRawScore = 800;

      Mock<IAttendanceRepository> mockAttendance;
      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);

      Mock<IAssignmentsRepository> mockAssignments;
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssignments
          .Setup(r => r.GetRawScore(_student, _course))
          .Returns(fullRawScore);

      _ = mockAttendance
          .Setup(r => r.GetAttendedClasses(_student, _course))
          .Throws<TimeoutException>();

      RatingCalculator calculator;
      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(_student, _course));
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_PropagatesException() {
      int fullAttendance;
      fullAttendance = 40;

      Mock<IAttendanceRepository> mockAttendance;
      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);

      Mock<IAssignmentsRepository> mockAssignments;
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssignments
          .Setup(r => r.GetRawScore(_student, _course))
          .Throws<TimeoutException>();

      _ = mockAttendance
          .Setup(r => r.GetAttendedClasses(_student, _course))
          .Returns(fullAttendance);

      RatingCalculator calculator;
      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(_student, _course));
    }
  }
}
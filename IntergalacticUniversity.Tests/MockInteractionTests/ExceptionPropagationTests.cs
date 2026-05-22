using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
    }

    [Test]
    public void CalculateTotalScore_CallsEachRepositoryExactlyOnce() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(15);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 20);

      mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrows_ExceptionPropagates() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Throws<TimeoutException>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(15);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(_student, _course));
    }
  }
}
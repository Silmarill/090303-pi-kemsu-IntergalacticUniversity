using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTests {
    private Student _student = null!;
    private Course _course = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse();
    }

    [Test]
    public void CalculateCurrentScore_WhenCalled_InvokesRepositoriesOnceWithSameArguments() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(10);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(300.0);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateCurrentScore(_student, _course);

      mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_WhenCalled_DoesNotInvokeRepositoriesMoreThanOnce() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(15);
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns(400.0);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 25);

      mockAttendance.Verify(r => r.GetAttendedClasses(_student, _course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(_student, _course), Times.Once);
    }
  }
}

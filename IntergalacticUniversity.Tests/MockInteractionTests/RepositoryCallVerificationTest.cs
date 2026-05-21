using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallVerificationTest {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator? _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateTestStudent();
      _course = TestDataFactory.CreateExamCourse();
      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
    }

    // Проверка 3.1: проверка вызова методов репозитория с правильными аргументами
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments() {
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _course)).Returns(20);
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _course)).Returns(500);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = _calculator.CalculateCurrentScore(_student, _course);

      _mockAttendance.Verify(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesWithCorrectArguments() {
      _ = _mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _course)).Returns(20);
      _ = _mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _course)).Returns(500);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);

      _ = _calculator.CalculateTotalScore(_student, _course, examOrCreditScore: 30);

      _mockAttendance.Verify(attendanceRepo => attendanceRepo.GetAttendedClasses(_student, _course), Times.Once);
      _mockAssignments.Verify(assignmentsRepo => assignmentsRepo.GetRawScore(_student, _course), Times.Once);
    }
  }
}

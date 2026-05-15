using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NoDuplicateRepositoryCallsTests {
    private static readonly double MaxRawAssignments = 800.0;
    private static readonly int TotalClassesCount = 40;
    private static readonly int MaxAttendanceScore = 20;
    private static readonly double SampleRawScore = 400.0;
    private static readonly int SampleAttended = 20;
    private static readonly double SampleExamScore = 30.0;

    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);

      _mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _mockAttendance.Setup(repository => repository.GetAttendedClasses(_student, _course))
          .Returns(SampleAttended);
      _mockAssignments.Setup(repository => repository.GetRawScore(_student, _course))
          .Returns(SampleRawScore);

      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateTotalScore_WhenCalled_InvokesEachRepositoryOnce() {
      _calculator.CalculateTotalScore(_student, _course, SampleExamScore);

      _mockAttendance.Verify(
          repository => repository.GetAttendedClasses(
              It.IsAny<Student>(),
              It.IsAny<Course>()),
          Times.Once);
      _mockAssignments.Verify(
          repository => repository.GetRawScore(
              It.IsAny<Student>(),
              It.IsAny<Course>()),
          Times.Once);
    }
  }
}

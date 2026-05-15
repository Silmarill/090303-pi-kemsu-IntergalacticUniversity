using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private static readonly double MaxRawAssignments = 1000.0;
    private static readonly int TotalClassesCount = 30;
    private static readonly int MaxAttendanceScore = 20;
    private static readonly int AttendedClassesCount = 30;
    private static readonly double SampleRawScore = 400.0;
    private static readonly double ExpectedAttendanceOnlyScore = 20.0;
    private static readonly double ExpectedAssignmentsOnlyScore = 16.0;

    [Test]
    public void CalculateCurrentScore_WhenRawScoreNull_ReturnsAttendanceOnly() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;

      student = TestDataFactory.CreateStudent();
      course = TestDataFactory.CreateExamCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);

      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      mockAssignments.Setup(repository => repository.GetRawScore(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Returns((double?)null);
      mockAttendance.Setup(repository => repository.GetAttendedClasses(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Returns(AttendedClassesCount);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double actualScore = calculator.CalculateCurrentScore(student, course);

      Assert.That(actualScore, Is.EqualTo(ExpectedAttendanceOnlyScore));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceNull_ReturnsAssignmentsOnly() {
      Student student;
      Course course;
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;

      student = TestDataFactory.CreateStudent();
      course = TestDataFactory.CreateExamCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);

      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      mockAssignments.Setup(repository => repository.GetRawScore(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Returns(SampleRawScore);
      mockAttendance.Setup(repository => repository.GetAttendedClasses(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Returns((int?)null);

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double actualScore = calculator.CalculateCurrentScore(student, course);

      Assert.That(actualScore, Is.EqualTo(ExpectedAssignmentsOnlyScore));
    }
  }
}

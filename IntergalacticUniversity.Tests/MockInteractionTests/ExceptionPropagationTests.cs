using System;
using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    private static readonly double MaxRawAssignments = 1000.0;
    private static readonly int TotalClassesCount = 30;
    private static readonly int MaxAttendanceScore = 20;

    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse(
          MaxRawAssignments,
          TotalClassesCount,
          MaxAttendanceScore);
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrows_PropagatesTimeoutException() {
      Mock<IAttendanceRepository> mockAttendance;
      Mock<IAssignmentsRepository> mockAssignments;
      RatingCalculator calculator;

      mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      mockAssignments.Setup(repository => repository.GetRawScore(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Returns((double?)null);
      mockAttendance.Setup(repository => repository.GetAttendedClasses(
              It.IsAny<Student>(),
              It.IsAny<Course>()))
          .Throws<TimeoutException>();

      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(_student, _course));
    }
  }
}

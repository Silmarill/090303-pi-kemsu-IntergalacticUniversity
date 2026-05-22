using System;
using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    private Student _student = null!;
    private Course _course = null!;

    [SetUp]
    public void SetUp() {
      _student = TestDataFactory.CreateStudent();
      _course = TestDataFactory.CreateExamCourse();
    }

    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrows_PropagatesException() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAssignments.Setup(r => r.GetRawScore(It.IsAny<Student>(), It.IsAny<Course>()))
          .Throws<TimeoutException>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(It.IsAny<Student>(), It.IsAny<Course>()))
          .Returns(5);
      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(_student, _course));
    }
  }
}

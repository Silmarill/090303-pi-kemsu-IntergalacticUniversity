using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionsTests {
  [TestFixture]
  public class MockInteractionTests {
    [Test]
    public void CalculateCurrentScore_CallsRepositoriesWithCorrectArguments() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_NullFromAssignmentsRepository_OnlyAttendanceCalculated() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(20.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_NullFromAttendanceRepository_OnlyAssignmentsCalculated() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(20.0));
    }

    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30);
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateTotalScore(student, course, 30.0);

      mockAttendance.Verify(r => r.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(r => r.GetRawScore(student, course), Times.Once);
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceRepositoryThrowsException_ExceptionPropagates() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Loose);

      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Throws<TimeoutException>();

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(() => calculator.CalculateCurrentScore(student, course), Throws.TypeOf<TimeoutException>());
    }

    [Test]
    public void CalculateCurrentScore_WhenAssignmentsRepositoryThrowsException_ExceptionPropagates() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>(MockBehavior.Strict);

      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Throws<TimeoutException>();

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      Assert.That(() => calculator.CalculateCurrentScore(student, course), Throws.TypeOf<TimeoutException>());
    }
  }
}
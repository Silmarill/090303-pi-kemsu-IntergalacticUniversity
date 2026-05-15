using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleTestExample {
    [Test]
    public void WhenExamAndFullMarks_ThenCurrentScoreEqualsMax() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(30); // 100%

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1000);    // 100%

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double current = calculator.CalculateCurrentScore(student, course);

      // Assert
      Assert.That(current, Is.EqualTo(60.0));
    }

    // ============= ТЕСТ 2 =============
    [Test]
    public void WhenNoData_ThenCurrentScoreZero() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double current = calculator.CalculateCurrentScore(student, course);

      // Assert
      Assert.That(current, Is.EqualTo(0.0));
    }

    // ============= ТЕСТ 3 =============
    [Test]
    public void WhenRawScoreExceedsMax_ThenCapsAtMaxCurrent() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20); // 100% посещаемости

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1200.0); // больше 100% заданий

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double current = calculator.CalculateCurrentScore(student, course);

      // Assert: для Credit курса максимум = 80
      Assert.That(current, Is.EqualTo(80.0));
    }

    // ============= ТЕСТ 4 =============
    [Test]
    public void WhenCreditCourseWithExam_ThenTotalScoreCalculatedCorrectly() {
      // Arrange
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(13); // 65% посещаемости

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(800.0); // 100% заданий

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      // Act
      double total = calculator.CalculateTotalScore(student, course, 20); // 20 баллов за зачёт

      // Assert: 75 (текущая) + 20 = 95
      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}
using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class NullHandlingTests {
    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test Student" };

      _examCourse = new Course {
        CourseId = 101,
        Name = "Exam Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _creditCourse = new Course {
        CourseId = 102,
        Name = "Credit Course",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 15
      };
    }

    [Test]
    public void CalculateCurrentScore_WhenRawScoreIsNull_CalculatesOnlyAttendance() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(40);
      mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(20.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenAttendanceIsNull_CalculatesOnlyAssignments() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns((int?)null);
      mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(1000);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _creditCourse);

      double maxAssignments = 80.0 - 15.0;
      Assert.That(result, Is.EqualTo(maxAssignments));
    }

    [Test]
    public void CalculateCurrentScore_WhenBothAreNull_ReturnsZero() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);
      mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0));
    }
  }
}
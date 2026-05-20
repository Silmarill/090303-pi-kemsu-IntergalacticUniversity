using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class Block1_1_MinimumValuesTests {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };

      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _student = null!;
      _course = null!;
      mockAttendance = null!;
      mockAssignments = null!;
      _calculator = null!;
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _ = mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((double?)null);
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);

      double result = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }
  }
}
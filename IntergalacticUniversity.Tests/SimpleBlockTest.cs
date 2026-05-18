using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class SimpleBlockTest {
    private Student _student;
    private Course _course;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void Setup() {
      _student = new Student { Id = 42 };
      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }


    [Test]
    public void NullTest() {
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns((int?)null);
      _mockAssignments.Setup(r => r.GetRawScore(_student, _course)).Returns((int?)null);

      double current = _calculator.CalculateCurrentScore(_student, _course);

      Assert.That(current, Is.EqualTo(0));
    }
  }
}

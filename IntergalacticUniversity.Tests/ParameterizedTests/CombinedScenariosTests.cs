using Moq;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenariosTests {
    [TestCase(0.0, 0.0, 0.0)]         // Низкий / Низкий
    [TestCase(40.0, 50.0, 25.5)]      // Средний / Средний
    [TestCase(100.0, 100.0, 60.0)]    // Высокий / Высокий
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpected(
        double rawPercent, double attendancePercent, double expectedCurrent) {

      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,         // maxCurrent = 60
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15       // На задачи остается 45
      };

      double calculatedRawScore = rawPercent / 100.0 * course.MaxRawAssignmentsScore;
      int calculatedAttended = (int)(attendancePercent / 100.0 * course.TotalClasses);

      Mock<IAssignmentsRepository> mockAssign = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _ = mockAssign.Setup(mock => mock.GetRawScore(student, course)).Returns(calculatedRawScore);

      Mock<IAttendanceRepository> mockAttend = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _ = mockAttend.Setup(mock => mock.GetAttendedClasses(student, course)).Returns(calculatedAttended);

      RatingCalculator calculator = new RatingCalculator(mockAttend.Object, mockAssign.Object);

      double actual = calculator.CalculateCurrentScore(student, course);

      Assert.That(actual, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
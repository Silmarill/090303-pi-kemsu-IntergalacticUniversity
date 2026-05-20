using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class CombinedScenariosTests {
    // Проверка 2.4 - комбинированный тест: разные проценты заданий и посещаемости
    // Exam: MaxRaw=600, TotalClasses=20, MaxAttendance=15
    // maxCurrent=60, maxAssignments=45, maxAttendance=15
    //
    // rawPercent - процент выполнения заданий (0..1)
    // attendPercent - процент посещаемости (0..1)
    // expectedCurrent - ожидаемая текущая успеваемость
    [TestCase(0.0, 0.0, 0.0)]      // низкий/низкий  => 0*45 + 0*15 = 0
    [TestCase(0.0, 1.0, 15.0)]     // низкий/высокий => 0*45 + 1*15 = 15
    [TestCase(1.0, 0.0, 45.0)]     // высокий/низкий => 1*45 + 0*15 = 45
    [TestCase(0.5, 0.5, 30.0)]     // средний/средний => 0.5*45 + 0.5*15 = 22.5+7.5 = 30
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        double rawPercent, double attendPercent, double expectedCurrent) {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 600,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };

      double rawScore = rawPercent * course.MaxRawAssignmentsScore;
      int attended = (int)(attendPercent * course.TotalClasses);

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      _ = mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(attended);

      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _ = mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(rawScore);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      double result = calculator.CalculateCurrentScore(student, course);

      Assert.That(result, Is.EqualTo(expectedCurrent));
    }
  }
}

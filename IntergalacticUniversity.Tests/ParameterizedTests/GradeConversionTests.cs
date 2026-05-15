using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    private const string GradeUnsatisfactory = "Неудовлетворительно";
    private const string GradeSatisfactory = "Удовлетворительно";
    private const string GradeGood = "Хорошо";
    private const string GradeExcellent = "Отлично";

    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TestCase(49, GradeUnsatisfactory)]
    [TestCase(51, GradeSatisfactory)]
    [TestCase(60, GradeSatisfactory)]
    [TestCase(66, GradeGood)]
    [TestCase(75, GradeGood)]
    [TestCase(86, GradeExcellent)]
    [TestCase(100, GradeExcellent)]
    public void ConvertToGrade_WhenBoundaryScore_ReturnsExpectedGrade(
        double totalScore,
        string expectedGrade) {
      string actualGrade = _calculator.ConvertToGrade(totalScore);

      Assert.That(actualGrade, Is.EqualTo(expectedGrade));
    }
  }
}

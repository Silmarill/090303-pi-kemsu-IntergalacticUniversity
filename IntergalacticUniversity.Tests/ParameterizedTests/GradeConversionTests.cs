using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      Mock<IAttendanceRepository> mockAttendance;
      mockAttendance = new Mock<IAttendanceRepository>();

      Mock<IAssignmentsRepository> mockAssignments;
      mockAssignments = new Mock<IAssignmentsRepository>();

      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [TearDown]
    public void TearDown() {
      _calculator = null!;
    }

    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_WithVariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      string actualGrade;
      actualGrade = _calculator.ConvertToGrade(totalScore);

      Assert.That(actualGrade, Is.EqualTo(expectedGrade));
    }
  }
}
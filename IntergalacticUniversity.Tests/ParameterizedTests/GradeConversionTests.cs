using Moq;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _calculator = new RatingCalculator(
          new Mock<IAttendanceRepository>().Object,
          new Mock<IAssignmentsRepository>().Object);
    }

    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_BoundaryValues_ReturnsExpectedGrade(int score, string expectedGrade) {
      string grade = _calculator.ConvertToGrade(score);

      Assert.That(grade, Is.EqualTo(expectedGrade));
    }
  }
}

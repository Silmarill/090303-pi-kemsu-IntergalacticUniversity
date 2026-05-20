using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class GradeConversionTests {
    private RatingCalculator _calculator = null!;

    [SetUp]
    public void SetUp() {
      _calculator = new RatingCalculator(null!, null!);
    }

    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      string result = _calculator.ConvertToGrade(totalScore);
      Assert.That(result, Is.EqualTo(expectedGrade));
    }
  }
}
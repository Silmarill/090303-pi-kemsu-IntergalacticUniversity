using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_BoundaryValues_ReturnsCorrectText(double score, string expectedGrade) {
      RatingCalculator calculator = new RatingCalculator(null, null);
      string actualGrade = calculator.ConvertToGrade(score);

      Assert.That(actualGrade, Is.EqualTo(expectedGrade));
    }
  }
}
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversationTests {
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      RatingCalculator calculator = new RatingCalculator(null, null);
      string result = calculator.ConvertToGrade(totalScore);
      Assert.That(result, Is.EqualTo(expectedGrade));
    }
  }
}

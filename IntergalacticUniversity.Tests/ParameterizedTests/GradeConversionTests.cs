using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(88, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_OnBoundaryValues_ReturnsCorrectGrade(double totalScore, string expectedGrade) {
      RatingCalculator calculator;
      string grade;

      calculator = new RatingCalculator(null, null);
      grade = calculator.ConvertToGrade(totalScore);

      Assert.That(grade, Is.EqualTo(expectedGrade));
    }
  }
}
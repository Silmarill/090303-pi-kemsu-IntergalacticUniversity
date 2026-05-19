using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class Block2_1_GradeConversionTests {
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_WithBoundaryValues_ReturnsCorrectGrade(double totalScore, string expectedGrade) {
      // Arrange
      RatingCalculator calculator = new RatingCalculator(null, null);

      // Act
      string result = calculator.ConvertToGrade(totalScore);

      // Assert
      Assert.That(result, Is.EqualTo(expectedGrade));
    }
  }
}
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTest {
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      // Создание с null, так как этот метод не использует зависимости
      _calculator = new RatingCalculator(null, null);
    }

    // Проверка 2.1: границы перевода баллов в оценку
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(50, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(65, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(70, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(85, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(90, "Отлично")]
    [TestCase(100, "Отлично")]

    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(double totalScore, string expectedGrade) {
      string result = _calculator.ConvertToGrade(totalScore);

      Assert.That(result, Is.EqualTo(expectedGrade));
    }
  }
}

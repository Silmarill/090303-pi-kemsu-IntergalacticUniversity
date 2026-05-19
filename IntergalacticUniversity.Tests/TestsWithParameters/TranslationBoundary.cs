//Сделал ИИ

using IntergalacticUniversity.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntergalacticUniversity.Tests.TestsWithParameters {
  public class TranslationBoundary {
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _calculator = new RatingCalculator(null, null);
    }

    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]

    public void ConvertToGrade_OnBoundaryValues_ReturnsCorrectGrade(int score, string expected) {
      string result = _calculator.ConvertToGrade(score);
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}

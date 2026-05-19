using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTests {
    // Проверка 2.1 - граничные значения перевода баллов в оценку
    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_BoundaryValues_ReturnsExpectedGrade(double score, string expected) {
      // ConvertToGrade не обращается к репозиториям, моки нужны только для конструктора
      RatingCalculator calculator = new RatingCalculator(
        new Mock<IAttendanceRepository>().Object,
        new Mock<IAssignmentsRepository>().Object
      );

      string result = calculator.ConvertToGrade(score);

      Assert.That(result, Is.EqualTo(expected));
    }
  }
}

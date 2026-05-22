// Сделал ИИ

using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.TestsWithParameters {
  [TestFixture]
  public class TranslationBoundary {
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignment = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(mockAttendance.Object, mockAssignment.Object);
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

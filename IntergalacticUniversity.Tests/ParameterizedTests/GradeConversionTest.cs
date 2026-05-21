using IntergalacticUniversity.Core.Interfaces;
using Moq;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.ParameterizedTests {
  [TestFixture]
  public class GradeConversionTest {
    private RatingCalculator _calculator = null!;
    private Mock<IAttendanceRepository>? _mockAttendance;
    private Mock<IAssignmentsRepository>? _mockAssignments;

    [SetUp]
    public void SetUp() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
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

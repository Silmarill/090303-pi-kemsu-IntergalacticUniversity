using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTests {
    private RatingCalculator _calculator;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;

    [SetUp]
    public void Setup() {
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    // Граница "Отлично" (86 - 100)
    [TestCase(100.0, "Отлично")]
    [TestCase(86.0, "Отлично")]
    [TestCase(85.9, "Хорошо")]

    // Граница "Хорошо" (66 - 85)
    [TestCase(75.0, "Хорошо")]
    [TestCase(66.0, "Хорошо")]
    [TestCase(65.9, "Удовлетворительно")]

    // Граница "Удовлетворительно" (51 - 65)
    [TestCase(60.0, "Удовлетворительно")]
    [TestCase(51.0, "Удовлетворительно")]
    [TestCase(50.9, "Неудовлетворительно")]

    // Граница "Неудовлетворительно" (0 - 50)
    [TestCase(49.0, "Неудовлетворительно")]
    [TestCase(0.0, "Неудовлетворительно")]
    public void ConvertToGrade_BoundaryValues_ReturnsExpectedGrade(double score, string expectedGrade) {
      // Act
      string result = _calculator.ConvertToGrade(score);

      // Assert
      Assert.That(result, Is.EqualTo(expectedGrade));
    }
  }
}
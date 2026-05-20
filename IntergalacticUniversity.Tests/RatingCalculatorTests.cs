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
      // Моки и калькулятор перед каждым тестом инициализируются
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void ConvertToGrade_Score86_ReturnsExcellent() {
      // Arrange
      double score = 86.0;

      // Act
      string result = _calculator.ConvertToGrade(score);

      // Assert
      Assert.That(result, Is.EqualTo("Отлично"));
    }

    [Test]
    public void ConvertToGrade_Score66_ReturnsGood() {
      // Arrange
      double score = 66.0;

      // Act
      string result = _calculator.ConvertToGrade(score);

      // Assert
      Assert.That(result, Is.EqualTo("Хорошо"));
    }

    [Test]
    public void ConvertToGrade_Score51_ReturnsSatisfactory() {
      // Arrange
      double score = 51.0;

      // Act
      string result = _calculator.ConvertToGrade(score);

      // Assert
      Assert.That(result, Is.EqualTo("Удовлетворительно"));
    }

    [Test]
    public void ConvertToGrade_Score50_ReturnsUnsatisfactory() {
      // Arrange
      double score = 50.0;

      // Act
      string result = _calculator.ConvertToGrade(score);

      // Assert
      Assert.That(result, Is.EqualTo("Неудовлетворительно"));
    }
  }
}
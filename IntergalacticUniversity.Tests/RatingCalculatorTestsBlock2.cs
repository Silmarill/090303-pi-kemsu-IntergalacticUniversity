using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests {
  [TestFixture]
  public class RatingCalculatorTestsBlock2 {
    private const double ExamMaxRawScore = 1000;
    private const int ExamTotalClasses = 40;
    private const int ExamMaxAttendanceScore = 20;

    private const double CreditMaxRawScore = 1000;
    private const int CreditTotalClasses = 40;
    private const int CreditMaxAttendanceScore = 10;
    private const int CreditMaxCurrent = 80;

    private const int CustomExamMaxRawScore = 600;
    private const int CustomExamTotalClasses = 15;
    private const int CustomExamMaxAttendanceScore = 15;

    private Student _student;
    private Course _examCourse;
    private Course _creditCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "На тесте студент" };

      _examCourse = new Course {
        CourseId = 1,
        Name = "Физика",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = ExamMaxRawScore,
        TotalClasses = ExamTotalClasses,
        MaxAttendanceScore = ExamMaxAttendanceScore
      };

      _creditCourse = new Course {
        CourseId = 2,
        Name = "Базы данных",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = CreditMaxRawScore,
        TotalClasses = CreditTotalClasses,
        MaxAttendanceScore = CreditMaxAttendanceScore,
        MaxCurrent = CreditMaxCurrent
      };

      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [TestCase(49, "Неудовлетворительно")]
    [TestCase(51, "Удовлетворительно")]
    [TestCase(60, "Удовлетворительно")]
    [TestCase(66, "Хорошо")]
    [TestCase(75, "Хорошо")]
    [TestCase(86, "Отлично")]
    [TestCase(100, "Отлично")]
    public void ConvertToGrade_VariousScores_ReturnsExpectedGrade(int score, string expectedGrade) {
      string result = _calculator.ConvertToGrade(score);
      Assert.That(result, Is.EqualTo(expectedGrade));
    }

    [TestCase(0, 0)]
    [TestCase(300, 12)]
    [TestCase(1000, 40)]
    public void CalculateCurrentScore_DifferentAssignmentPercentages_ReturnsCorrectAssignmentPart(
        double rawScore, double expectedAssignmentScore) {
      const int MaxAttendance = 40;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(MaxAttendance);

      double expectedTotal = expectedAssignmentScore + ExamMaxAttendanceScore;
      double result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }

    [TestCase(40, 10)]
    [TestCase(20, 5)]
    [TestCase(0, 0)]
    public void CalculateCurrentScore_DifferentAttendancePercentages_ReturnsCorrectAttendancePart(
        int attendedClasses, double expectedAttendanceScore) {
      const double MaxRawScore = 1000;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _creditCourse)).Returns(MaxRawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _creditCourse)).Returns(attendedClasses);

      const double AssignmentMaxScoreForCredit = 70;
      double expectedTotal = AssignmentMaxScoreForCredit + expectedAttendanceScore;
      double result = _calculator.CalculateCurrentScore(_student, _creditCourse);

      Assert.That(result, Is.EqualTo(expectedTotal).Within(0.001));
    }

    [TestCase(0, 0, 0)]
    [TestCase(600, 15, 60)]
    [TestCase(300, 7, 29.5)]
    [TestCase(400, 8, 38)]
    [TestCase(500, 10, 47.5)]
    public void CalculateCurrentScore_CombinedPercentages_ReturnsExpectedCurrent(
        double rawScore, int attended, double expectedCurrent) {
      Course customCourse = new Course {
        CourseId = 3,
        Name = "Комбинированный курс",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = CustomExamMaxRawScore,
        TotalClasses = CustomExamTotalClasses,
        MaxAttendanceScore = CustomExamMaxAttendanceScore
      };

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, customCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, customCourse)).Returns(attended);

      double result = _calculator.CalculateCurrentScore(_student, customCourse);
      Assert.That(result, Is.EqualTo(expectedCurrent).Within(0.001));
    }
  }
}
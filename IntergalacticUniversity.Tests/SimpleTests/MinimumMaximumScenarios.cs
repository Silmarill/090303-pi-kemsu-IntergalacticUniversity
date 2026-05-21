// ДипСик помог покрыть граничные случаи: null данные, максимальные значения и переполнение сверху
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student _student;
    private Course _examCourse;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Mock<IAssignmentsRepository> _mockAssignments;
    private RatingCalculator _calculator;

    [SetUp]
    public void SetUp() {
      _student = new Student { Id = 1, Name = "Test Student" };
      _examCourse = new Course {
        CourseId = 101,
        Name = "Test Course",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAssignments = new Mock<IAssignmentsRepository>();
      _calculator = new RatingCalculator(_mockAttendance.Object, _mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns((double?)null);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns((int?)null);

      double result;
      result = _calculator.CalculateCurrentScore(_student, _examCourse);

      Assert.That(result, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void CalculateCurrentScore_WhenMaxValues_Returns60() {
      double rawScore;
      int attendedClasses;
      double expectedCurrent;
      string expectedGrade;
      double current;
      string grade;

      rawScore = 800.0;
      attendedClasses = 40;
      expectedCurrent = 60.0;
      expectedGrade = "Отлично";

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, _examCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _examCourse)).Returns(attendedClasses);

      current = _calculator.CalculateCurrentScore(_student, _examCourse);
      grade = _calculator.ConvertToGrade(100.0);

      Assert.That(current, Is.EqualTo(expectedCurrent).Within(0.001));
      Assert.That(grade, Is.EqualTo(expectedGrade));
    }

    [Test]
    public void CalculateCurrentScore_WhenOverflow_CapsAtMaxCurrent() {
      Course creditCourse;
      double rawScore;
      int attendedClasses;
      double expectedResult;
      double result;

      creditCourse = new Course {
        CourseId = 102,
        Name = "Credit Course",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      rawScore = 1200.0;
      attendedClasses = 30;
      expectedResult = 80.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(attendedClasses);

      result = _calculator.CalculateCurrentScore(_student, creditCourse);

      Assert.That(result, Is.EqualTo(expectedResult).Within(0.001));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_AddsExamOrCreditScoreCapped() {
      Course creditCourse;
      double rawScore;
      int attendedClasses;
      double examScore;
      double expectedTotal;
      double total;

      creditCourse = new Course {
        CourseId = 103,
        Name = "Credit Course",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 10
      };

      rawScore = 928.57;
      attendedClasses = 40;
      examScore = 20.0;
      expectedTotal = 95.0;

      _ = _mockAssignments.Setup(r => r.GetRawScore(_student, creditCourse)).Returns(rawScore);
      _ = _mockAttendance.Setup(r => r.GetAttendedClasses(_student, creditCourse)).Returns(attendedClasses);

      total = _calculator.CalculateTotalScore(_student, creditCourse, examScore);

      Assert.That(total, Is.EqualTo(expectedTotal).Within(0.001));
    }
  }
}
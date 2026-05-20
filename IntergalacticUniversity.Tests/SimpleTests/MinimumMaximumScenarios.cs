using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Mock<IAttendanceRepository> _attendanceMock;
    private Mock<IAssignmentsRepository> _assignmentsMock;
    private RatingCalculator _calculator;
    private Student _testStudent;

    [SetUp]
    public void SetUp() {
      // Инициализация моков перед каждым тестом
      _attendanceMock = new Mock<IAttendanceRepository>(MockBehavior.Strict);
      _assignmentsMock = new Mock<IAssignmentsRepository>(MockBehavior.Strict);
      _calculator = new RatingCalculator(_attendanceMock.Object, _assignmentsMock.Object);
      _testStudent = new Student { Id = 1, Name = "Тестовый Студент" };
    }

    [Test]
    public void Test_1_1_MinValues_EverythingAtNull_ReturnsZero() {
      Course course = new Course {
        CourseId = 1,
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 20,
        MaxAttendanceScore = 10
      };

      // Репозитории возвращают null т.к. данные отсутствуют
      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns((double?)null);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns((int?)null);

      double currentScore = _calculator.CalculateCurrentScore(_testStudent, course);

      Assert.That(currentScore, Is.EqualTo(0.0).Within(0.001));
    }

    [Test]
    public void Test_1_2_MaxValues_FullProgress_ReturnsMaxAndExcellent() {
      Course course = new Course {
        CourseId = 2,
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };

      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns(800.0);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns(40);

      double currentScore = _calculator.CalculateCurrentScore(_testStudent, course);
      string grade = _calculator.ConvertToGrade(100.0);

      Assert.That(currentScore, Is.EqualTo(60.0).Within(0.001));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void Test_1_3_UpperBoundRestriction_OverHundredPercent_RestrictsToMax() {
      Course course = new Course {
        CourseId = 3,
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 30,
        MaxAttendanceScore = 15
      };

      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns(1200.0);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns(30);

      double currentScore = _calculator.CalculateCurrentScore(_testStudent, course);

      // Ожидается 80 баллов. 65 за задачи + 15 за посещаемость
      Assert.That(currentScore, Is.EqualTo(80.0).Within(0.001));
    }

    [Test]
    public void Test_1_4_TotalScoreWithCredit_CorrectSummation() {
      Course course = new Course {
        CourseId = 4,
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 100,
        TotalClasses = 10,
        MaxAttendanceScore = 20
      };

      _ = _assignmentsMock.Setup(mock => mock.GetRawScore(_testStudent, course)).Returns(91.6666);
      _ = _attendanceMock.Setup(mock => mock.GetAttendedClasses(_testStudent, course)).Returns(10);

      double totalScore = _calculator.CalculateTotalScore(_testStudent, course, 20.0);

      Assert.That(totalScore, Is.EqualTo(95.0).Within(0.1));
    }
  }
}
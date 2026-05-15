using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.SimpleTests {
  [TestFixture]
  public class MinimumMaximumScenarios {
    private Student student;
    private Mock<IAttendanceRepository> mockAttendance;
    private Mock<IAssignmentsRepository> mockAssignments;
    private RatingCalculator calculator;

    [SetUp]
    public void Setup() {
      student = new Student { Id = 1, Name = "Test Student" };
      mockAttendance = new Mock<IAttendanceRepository>();
      mockAssignments = new Mock<IAssignmentsRepository>();
      calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
    }

    [Test]
    public void CalculateCurrentScore_WhenNoData_ReturnsZero() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns((double?)null);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns((int?)null);

      double current = calculator.CalculateCurrentScore(student, course);
      Assert.That(current, Is.EqualTo(0.0));
    }

    [Test]
    public void CalculateCurrentScore_WhenFullMarks_ReturnsMaxCurrentAndConvertToGrade_ReturnsExcellent() {
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 800.0,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(800.0);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(40);

      double current = calculator.CalculateCurrentScore(student, course);
      string grade = calculator.ConvertToGrade(100.0);
      Assert.That(current, Is.EqualTo(60.0));
      Assert.That(grade, Is.EqualTo("Отлично"));
    }

    [Test]
    public void CalculateCurrentScore_ForCreditCourse_WhenOverflow_DoesNotExceedMaxCurrent() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(1200.0);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20);

      double current = calculator.CalculateCurrentScore(student, course);
      Assert.That(current, Is.LessThanOrEqualTo(80.0));
    }

    [Test]
    public void CalculateTotalScore_ForCreditCourse_WhenFullCredit_ReturnsCappedSum() {
      Course course = new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 20,
        MaxAttendanceScore = 15
      };
      mockAssignments.Setup(r => r.GetRawScore(student, course)).Returns(923.0);
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course)).Returns(20);

      double total = calculator.CalculateTotalScore(student, course, examOrCreditScore: 20.0);
      Assert.That(total, Is.EqualTo(95.0));
    }
  }
}
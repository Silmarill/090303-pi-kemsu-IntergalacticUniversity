//16 строку помогал писать ИИ


using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntergalacticUniversity.Tests.TestsWithParameters {
  public class ParameterizationOfTaskScores {
    private RatingCalculator _calculator;
    private Mock<IAttendanceRepository> _mockAttendance;
    private Student _student;
    private Course _course;

    [SetUp]
    public void SetUp() {
      _calculator = new RatingCalculator(null, null);
      _mockAttendance = new Mock<IAttendanceRepository>();
      _mockAttendance.Setup(r => r.GetAttendedClasses(_student, _course)).Returns(_course.TotalClasses);

      _course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000,
        TotalClasses = 40,
        MaxAttendanceScore = 20
      };
    }

    [TestCase(0,0)]
    [TestCase(300,12)]
    [TestCase(1000,40)]
 
    public void TaskScore(int score, int expected) {
      int result = _calculator.CalculateCurrentScore(score,expected);
      Assert.That(result, Is.EqualTo(expected));
    }
  }
}

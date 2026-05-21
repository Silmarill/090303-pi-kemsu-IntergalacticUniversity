using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;
using IntergalacticUniversity.Tests.Common;
using Moq;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class RepositoryCallCountTest {
    // Проверка 3.3: проверка, что метод CalculateTotalScore не вызывает репозитории повторно
    [Test]
    public void CalculateTotalScore_CallsRepositoriesOnlyOnce() {
      Student student = TestDataFactory.CreateTestStudent();
      Course course = TestDataFactory.CreateExamCourse();

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course)).Returns(20);
      _ = mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);

      mockAttendance.Verify(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course), Times.Once);
      mockAssignments.Verify(assignmentsRepo => assignmentsRepo.GetRawScore(student, course), Times.Once);
    }

    [Test]
    public void CalculateTotalScore_WhenCalledMultipleTimes_CallsRepositoriesEachTime() {
      Student student = TestDataFactory.CreateTestStudent();
      Course course = TestDataFactory.CreateExamCourse();

      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();

      _ = mockAttendance.Setup(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course)).Returns(20);
      _ = mockAssignments.Setup(assignmentsRepo => assignmentsRepo.GetRawScore(student, course)).Returns(500);

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);

      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: 30);
      _ = calculator.CalculateTotalScore(student, course, examOrCreditScore: 25);

      mockAttendance.Verify(attendanceRepo => attendanceRepo.GetAttendedClasses(student, course), Times.Exactly(2));
      mockAssignments.Verify(assignmentsRepo => assignmentsRepo.GetRawScore(student, course), Times.Exactly(2));
    }
  }
}

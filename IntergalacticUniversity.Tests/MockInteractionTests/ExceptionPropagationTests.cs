using System;
using Moq;
using NUnit.Framework;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity.Tests.MockInteractionTests {
  [TestFixture]
  public class ExceptionPropagationTests {
    [Test]
    public void CalculateCurrentScore_WhenRepositoryThrowsException_PropagatesException() {
      Student student = new Student { Id = 1 };
      Course course = new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = 1000.0,
        TotalClasses = 30,
        MaxAttendanceScore = 20
      };
      Mock<IAttendanceRepository> mockAttendance = new Mock<IAttendanceRepository>();
      Mock<IAssignmentsRepository> mockAssignments = new Mock<IAssignmentsRepository>();
      mockAttendance.Setup(r => r.GetAttendedClasses(student, course))
                    .Throws(new TimeoutException("Database timeout"));

      RatingCalculator calculator = new RatingCalculator(mockAttendance.Object, mockAssignments.Object);
      Assert.Throws<TimeoutException>(() => calculator.CalculateCurrentScore(student, course));
    }
  }
}
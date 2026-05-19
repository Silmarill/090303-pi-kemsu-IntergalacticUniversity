using IntergalacticUniversity.Core.Models;

namespace IntergalacticUniversity.Tests.Common {
  public static class TestDataFactory {
    public static Student CreateStudent(int id = 1, string name = "Test Student") {
      return new Student { Id = id, Name = name };
    }

    public static Course CreateExamCourse(
        double maxRaw = 1000,
        int totalClasses = 30,
        int maxAttendance = 20) {
      return new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance,
      };
    }

    public static Course CreateCreditCourse(
        double maxRaw = 1000,
        int totalClasses = 20,
        int maxAttendance = 15) {
      return new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance,
      };
    }
  }
}

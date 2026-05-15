using IntergalacticUniversity.Core.Models;

namespace IntergalacticUniversity.Tests.Common {
  public static class TestDataFactory {
    public static readonly int DefaultStudentId = 1;

    public static Student CreateStudent() {
      return new Student { Id = DefaultStudentId };
    }

    public static Course CreateExamCourse(
        double maxRawAssignmentsScore,
        int totalClasses,
        int maxAttendanceScore) {
      return new Course {
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };
    }

    public static Course CreateCreditCourse(
        double maxRawAssignmentsScore,
        int totalClasses,
        int maxAttendanceScore) {
      return new Course {
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };
    }
  }
}

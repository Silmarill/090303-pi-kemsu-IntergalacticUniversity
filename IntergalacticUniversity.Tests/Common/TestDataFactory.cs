using IntergalacticUniversity.Core.Models;

namespace IntergalacticUniversity.Tests.Common {
  public static class TestDataFactory {
    public static Student CreateTestStudent() {
      return new Student {
        Id = 1,
        Name = "Тестовый Студент"
      };
    }

    public static Course CreateExamCourse(double maxRawAssignmentsScore = 1000, int totalClasses = 40, int maxAttendanceScore = 20) {
      return new Course {
        CourseId = 101,
        Name = "Тестовый курс (экзамен)",
        Type = ExamType.Exam,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };
    }

    public static Course CreateCreditCourse(double maxRawAssignmentsScore = 1000, int totalClasses = 40, int maxAttendanceScore = 10) {
      return new Course {
        CourseId = 102,
        Name = "Тестовый курс (зачёт)",
        Type = ExamType.Credit,
        MaxRawAssignmentsScore = maxRawAssignmentsScore,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendanceScore
      };
    }
  }
}

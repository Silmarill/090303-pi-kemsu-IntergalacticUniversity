using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Interfaces;

namespace IntergalacticUniversity.Core.Services {
  public class RatingCalculator {
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IAssignmentsRepository _assignmentsRepo;

    public RatingCalculator(IAttendanceRepository attendanceRepo,
                            IAssignmentsRepository assignmentsRepo) {
      _attendanceRepo = attendanceRepo;
      _assignmentsRepo = assignmentsRepo;
    }

    public double CalculateCurrentScore(Student student, Course course) {

      int maxCurrent = 80;
      if (course.Type == ExamType.Exam) {
        maxCurrent = 60;
      }

      int maxAttendance = course.MaxAttendanceScore;
      int maxAssignments = maxCurrent - maxAttendance;

      double? raw = _assignmentsRepo.GetRawScore(student, course);
      double assignmentsScore = 0;
      if (raw.HasValue && course.MaxRawAssignmentsScore > 0) {
        assignmentsScore = raw.Value / course.MaxRawAssignmentsScore * maxAssignments;
        assignmentsScore = Math.Min(assignmentsScore, maxAssignments);
      }

      int? attended = _attendanceRepo.GetAttendedClasses(student, course);
      double attendanceScore = 0;
      if (attended.HasValue && course.TotalClasses > 0) {
        double percent = (double)attended.Value / course.TotalClasses;
        attendanceScore = percent * maxAttendance;
      }

      return assignmentsScore + attendanceScore;
    }

    public double CalculateTotalScore(Student student, Course course, double? examOrCreditScore = null) {
      double current = CalculateCurrentScore(student, course);
      if (!examOrCreditScore.HasValue) {
        return current;
      }

      double maxExam = (course.Type == ExamType.Exam) ? 40.0 : 20.0;
      double examScore = Math.Max(0, Math.Min(examOrCreditScore.Value, maxExam));
      double total = current + examScore;
      return Math.Min(total, 100.0);
    }

    public string ConvertToGrade(double totalScore) {
      if (totalScore >= 86) {
        return "Отлично";
      }

      if (totalScore >= 66) {
        return "Хорошо";
      }

      if (totalScore >= 51) {
        return "Удовлетворительно";
      }

      return "Неудовлетворительно";
    }
  }
}
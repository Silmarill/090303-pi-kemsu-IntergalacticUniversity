using IntergalacticUniversity.Core.Models;

namespace IntergalacticUniversity.Core.Interfaces {
  public interface IAttendanceRepository {
    int? GetAttendedClasses(Student student, Course course);
  }
}
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;

// === Заглушки репозиториев, чтобы программа компилировалась ===
public class DummyAttendanceRepository : IAttendanceRepository {
  private int? _attended;

  public void SetAttended(int? attended) {
    _attended = attended;
  }

  public int? GetAttendedClasses(Student student, Course course) {
    return _attended;
  }
}
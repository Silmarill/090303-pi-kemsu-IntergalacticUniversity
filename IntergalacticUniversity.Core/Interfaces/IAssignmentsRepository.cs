using IntergalacticUniversity.Core.Models;

namespace IntergalacticUniversity.Core.Interfaces {
  public interface IAssignmentsRepository {
    double? GetRawScore(Student student, Course course);
  }
}
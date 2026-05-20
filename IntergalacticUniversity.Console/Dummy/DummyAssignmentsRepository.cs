using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;

public class DummyAssignmentsRepository : IAssignmentsRepository {
  private double? _rawScore;

  public void SetRawScore(double? score) {
    _rawScore = score;
  }

  public double? GetRawScore(Student student, Course course) {
    return _rawScore;
  }
}
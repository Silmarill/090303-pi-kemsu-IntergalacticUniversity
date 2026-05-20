namespace IntergalacticUniversity.Core.Models {
  /// <summary>
  /// Виды оценки.
  /// </summary>
  public enum ExamType {
    /// <summary>
    /// Экзамен.
    /// </summary>
    Exam,

    /// <summary>
    /// Зачет.
    /// </summary>
    Credit
  }

  public class Course {
    public int CourseId { get; set; }

    public string Name { get; set; }

    public ExamType Type { get; set; }

    public double MaxRawAssignmentsScore { get; set; }

    public int TotalClasses { get; set; }

    public int MaxAttendanceScore { get; set; }
  }
}
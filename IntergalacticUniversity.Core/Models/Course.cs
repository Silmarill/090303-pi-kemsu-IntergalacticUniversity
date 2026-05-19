namespace IntergalacticUniversity.Core.Models {
  public enum ExamType {
    Exam, Credit
  }

  public class Course {
    public int CourseId { get; set; }
    public string Name { get; set; }
    public ExamType Type { get; set; }
    public double MaxRawAssignmentsScore { get; set; }
    public int TotalClasses { get; set; }
    public int MaxAttendanceScore { get; set; }
    public int MaxCurrent { get; set; }
  }
}
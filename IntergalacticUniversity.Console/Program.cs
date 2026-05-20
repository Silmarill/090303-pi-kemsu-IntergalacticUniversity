using System;
using IntergalacticUniversity.Core.Interfaces;
using IntergalacticUniversity.Core.Models;
using IntergalacticUniversity.Core.Services;

namespace IntergalacticUniversity {
  public class Program {
    public static void Main() {
      Console.OutputEncoding = System.Text.Encoding.UTF8;
      Console.WriteLine("=== Система расчёта рейтинга Межгалактического Университета ===\n");

      IAttendanceRepository attendanceRepo = new DummyAttendanceRepository();
      IAssignmentsRepository assignmentsRepo = new DummyAssignmentsRepository();
      RatingCalculator calculator = new RatingCalculator(attendanceRepo, assignmentsRepo);

      Student student = new Student { Id = 1, Name = "Алексей Звёздный" };

      Console.WriteLine("Выберите тип аттестации: 1 - Экзамен, 2 - Зачёт");
      string choice = Console.ReadLine();

      ExamType examType = ExamType.Exam;
      double challengeMaxScore = 40;
      double currentMaxScore = 60;

      if (choice == "2") {
        examType = ExamType.Credit;
        challengeMaxScore = 20;
        currentMaxScore = 80;
      }

      Console.Write("Введите максимально возможные баллы за все задания курса: ");
      double maxRaw = double.Parse(Console.ReadLine());

      Console.Write($"Введите текщие баллы за задания (от 0 до {maxRaw}): ");
      double rawScore = double.Parse(Console.ReadLine());

      Console.Write("Общее количество пар по дисциплине: ");
      int totalClasses = int.Parse(Console.ReadLine());

      Console.Write($"Сколько занятий посетил студент (от 0 до {totalClasses}): ");
      int attended = int.Parse(Console.ReadLine());

      Console.Write($"Максимальный балл за посещаемость (от 10 до 20) из {currentMaxScore}: ");
      int maxAttendance = int.Parse(Console.ReadLine());

      if (examType == ExamType.Exam) {
        Console.Write("Введите оценку за экзамен (0-40): ");
      } else {
        Console.Write("Введите балл за зачёт (0-20): ");
      }

      double examScore = double.Parse(Console.ReadLine());

      Course course = new Course {
        CourseId = 101,
        Name = "Практическая астрогация",
        Type = examType,
        MaxRawAssignmentsScore = maxRaw,
        TotalClasses = totalClasses,
        MaxAttendanceScore = maxAttendance
      };

      ((DummyAttendanceRepository)attendanceRepo).SetAttended(attended);
      ((DummyAssignmentsRepository)assignmentsRepo).SetRawScore(rawScore);

      double currentScore = calculator.CalculateCurrentScore(student, course);
      double totalScore = calculator.CalculateTotalScore(student, course, examScore);
      string grade = calculator.ConvertToGrade(totalScore);

      Console.WriteLine("\n========================================");
      Console.WriteLine($"Студент: {student.Name}");
      Console.WriteLine($"Курс: {course.Name} ({course.Type})");

      if (totalClasses > 0) {
        Console.WriteLine($"Посещаемость: {attended} / {totalClasses} ({100.0 * attended / totalClasses:F1}% = {1.0 * attended / totalClasses * maxAttendance:F0} баллов из {maxAttendance})");
      } else {
        Console.WriteLine($"Посещаемость: {attended} / 0 (0% = 0 баллов из {maxAttendance})");
      }

      double rawPercent = 0;
      if (maxRaw > 0) {
        rawPercent = rawScore / maxRaw * 100;
      }

      Console.WriteLine($"Посещаемость занимает {maxAttendance} баллов из {currentMaxScore}. На задания остается {currentMaxScore - maxAttendance:F0}");
      if (maxRaw > 0) {
        Console.WriteLine($"Баллы за задания: {rawScore} / {maxRaw} ({rawPercent:F1}% = {1.0 * rawScore / maxRaw * (currentMaxScore - maxAttendance):F0} баллов из {currentMaxScore - maxAttendance})");
      } else {
        Console.WriteLine($"Баллы за задания: {rawScore} / 0 (0% = 0 баллов из {currentMaxScore - maxAttendance})");
      }

      Console.WriteLine($"Текущая успеваемость: {currentScore:F0} из {currentMaxScore}");
      Console.WriteLine($"Итого за промежуточную аттестацию: {examScore} из {challengeMaxScore}");

      Console.WriteLine($"Итоговый суммарный балл: {totalScore:F0} из 100");
      Console.WriteLine($"Оценка: {grade}");
      Console.WriteLine("========================================\n");

      Console.WriteLine("Нажмите любую клавишу для выхода...");
      _ = Console.ReadKey();
    }
  }
}
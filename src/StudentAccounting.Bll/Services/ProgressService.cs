using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;

namespace StudentAccounting.Bll.Services;

/// <summary>
/// Бизнес-логика учёта успеваемости и посещаемости (слой BLL).
/// </summary>
public class ProgressService
{
    private readonly IRepository<Student> _students;
    private readonly IRepository<Grade> _grades;
    private readonly IRepository<Attendance> _attendances;

    public ProgressService(IRepository<Student> students,
        IRepository<Grade> grades, IRepository<Attendance> attendances)
    {
        _students = students;
        _grades = grades;
        _attendances = attendances;
    }

    /// <summary>Оценки студента с указанием дисциплины.</summary>
    public IReadOnlyList<(Discipline Discipline, int Value)> GetGrades(int studentId) =>
        _grades.Find(g => g.StudentId == studentId)
            .OrderBy(g => g.Semester)
            .Select(g => (g.Discipline!, g.Value))
            .ToList();

    /// <summary>Средний балл студента (0, если оценок нет).</summary>
    public double GetAverageGrade(int studentId)
    {
        var values = _grades.Find(g => g.StudentId == studentId).Select(g => (double)g.Value).ToList();
        return values.Count == 0 ? 0 : values.Average();
    }

    /// <summary>Средний балл по группе по всем оценкам её студентов.</summary>
    public double GetGroupAverageGrade(int groupId)
    {
        var ids = _students.Find(s => s.GroupId == groupId).Select(s => s.Id).ToHashSet();
        var values = _grades.Find(g => ids.Contains(g.StudentId)).Select(g => (double)g.Value).ToList();
        return values.Count == 0 ? 0 : values.Average();
    }

    /// <summary>Процент посещаемости студента (0, если отметок нет).</summary>
    public double GetAttendanceRate(int studentId)
    {
        var records = _attendances.Find(a => a.StudentId == studentId);
        return records.Count == 0 ? 0 : 100.0 * records.Count(a => a.IsPresent) / records.Count;
    }

    /// <summary>Определяет, является ли студент «отличником» (средний балл не ниже 4.5).</summary>
    public bool IsExcellent(int studentId) => GetAverageGrade(studentId) >= 4.5;
}

using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;

namespace StudentAccounting.Bll.Scholarship;

/// <summary>
/// Сервис расчёта стипендий (слой BLL).
/// Зависит только от абстракций: репозитории (IRepository), правила
/// назначения стипендии (IScholarshipPolicy) и экспорт отчёта (IReportExporter).
/// Каждая зависимость можно подменить в модульных тестах (Moq).
/// </summary>
public class ScholarshipService
{
    private readonly IRepository<Student> _students;
    private readonly IRepository<Group> _groups;
    private readonly IRepository<Grade> _grades;
    private readonly IScholarshipPolicy _policy;
    private readonly IReportExporter _exporter;

    public ScholarshipService(IRepository<Student> students,
        IRepository<Group> groups,
        IRepository<Grade> grades,
        IScholarshipPolicy policy,
        IReportExporter exporter)
    {
        _students = students ?? throw new ArgumentNullException(nameof(students));
        _groups = groups ?? throw new ArgumentNullException(nameof(groups));
        _grades = grades ?? throw new ArgumentNullException(nameof(grades));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
    }

    /// <summary>Формирует отчёт о стипендиях по группе.</summary>
    public ScholarshipReport BuildReport(int groupId)
    {
        var group = _groups.GetById(groupId)
            ?? throw new InvalidOperationException($"Группа с Id={groupId} не найдена");

        var report = new ScholarshipReport
        {
            GroupId = group.Id,
            GroupName = group.Name,
            GeneratedAt = DateTime.Now
        };

        foreach (var student in _students.Find(s => s.GroupId == groupId).OrderBy(s => s.LastName))
        {
            var grades = _grades.Find(g => g.StudentId == student.Id);
            var hasGrades = grades.Count > 0;
            var average = hasGrades ? grades.Average(g => (double)g.Value) : 0;
            var coefficient = _policy.GetCoefficient(average);
            var amount = decimal.Round(_policy.BaseAmount * coefficient, 2);

            report.Rows.Add(new ScholarshipReportRow
            {
                StudentId = student.Id,
                FullName = student.FullName(),
                AverageGrade = average,
                Coefficient = coefficient,
                Amount = amount,
                Rank = GetRank(average, hasGrades)
            });
            report.TotalAmount += amount;
        }

        return report;
    }

    /// <summary>Экспортирует отчёт выбранным форматировщиком.</summary>
    public string Export(ScholarshipReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return _exporter.Export(report);
    }

    /// <summary>
    /// Безопасный вариант построения отчёта: возвращает false и текст ошибки
    /// вместо пробрасывания исключения (используется слоем UI).
    /// </summary>
    public bool TryBuildReport(int groupId, out ScholarshipReport? report, out string? error)
    {
        try
        {
            report = BuildReport(groupId);
            error = null;
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            report = null;
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Определяет учебный ранг студента по среднему баллу.</summary>
    public static string GetRank(double averageGrade, bool hasGrades)
    {
        if (!hasGrades)
            return "нет оценок";
        if (averageGrade >= StandardScholarshipPolicy.ExcellentThreshold)
            return "отличник";
        if (averageGrade >= StandardScholarshipPolicy.GoodThreshold)
            return "хорошист";
        return "требует внимания";
    }
}

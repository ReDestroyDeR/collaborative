using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Storage;

namespace StudentAccounting.Bll.Services;

/// <summary>
/// Бизнес-логика работы со студентами (слой BLL).
/// Инкапсулирует правила предметной области и обращается к слою DAL.
/// </summary>
public class StudentService
{
    private readonly UniversityContext _context;

    public StudentService(UniversityContext context) => _context = context;

    /// <summary>Возвращает список всех студентов.</summary>
    public IReadOnlyList<Student> GetAll() => _context.Students;

    /// <summary>Находит студента по идентификатору.</summary>
    public Student? GetById(int id) =>
        _context.Students.FirstOrDefault(s => s.Id == id);

    /// <summary>Возвращает студентов указанной группы.</summary>
    public IReadOnlyList<Student> GetByGroup(int groupId) =>
        _context.Students.Where(s => s.GroupId == groupId).ToList();

    /// <summary>Ищет студентов по началу фамилии (без учёта регистра).</summary>
    public IReadOnlyList<Student> SearchByLastName(string lastNamePrefix) =>
        _context.Students
            .Where(s => s.LastName.StartsWith(lastNamePrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>Зачисляет нового студента в группу с проверкой бизнес-правил.</summary>
    public Student Enroll(string lastName, string firstName, string? middleName,
        string email, DateTime birthDate, int groupId)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия не может быть пустой", nameof(lastName));
        if (!email.Contains('@'))
            throw new ArgumentException("Некорректный адрес электронной почты", nameof(email));
        var group = _context.Groups.FirstOrDefault(g => g.Id == groupId)
            ?? throw new InvalidOperationException($"Группа с Id={groupId} не найдена");

        var number = _context.Students.Count(s => s.GroupId == groupId) + 1;
        var student = new Student
        {
            Id = _context.Students.Count == 0 ? 1 : _context.Students.Max(s => s.Id) + 1,
            LastName = lastName.Trim(),
            FirstName = firstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            Email = email.Trim(),
            BirthDate = birthDate,
            RecordBookNumber = $"{group.Name}-{number:D3}",
            EnrolledDate = DateTime.Today,
            Status = StudentStatus.Active,
            GroupId = groupId,
            Group = group
        };
        _context.Students.Add(student);
        group.Students.Add(student);
        return student;
    }

    /// <summary>Вычисляет средний балл студента по всем оценкам.</summary>
    public double GetAverageGrade(int studentId) =>
        _context.Grades.Where(g => g.StudentId == studentId)
            .Select(g => (double)g.Value)
            .DefaultIfEmpty(0)
            .Average();

    /// <summary>Вычисляет средний балл группы по всем оценкам её студентов.</summary>
    public double GetGroupAverageGrade(int groupId)
    {
        var studentIds = _context.Students.Where(s => s.GroupId == groupId).Select(s => s.Id).ToHashSet();
        var grades = _context.Grades.Where(g => studentIds.Contains(g.StudentId)).ToList();
        return grades.Count == 0 ? 0 : grades.Average(g => g.Value);
    }

    /// <summary>Процент посещаемости студента за всё время.</summary>
    public double GetAttendanceRate(int studentId)
    {
        var records = _context.Attendances.Where(a => a.StudentId == studentId).ToList();
        return records.Count == 0 ? 0 : 100.0 * records.Count(a => a.IsPresent) / records.Count;
    }
}

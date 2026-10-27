using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;

namespace StudentAccounting.Bll.Services;

/// <summary>
/// Бизнес-логика работы со студентами (слой BLL).
/// Сервис зависит только от абстракции IRepository&lt;Student&gt; и не знает,
/// как физически организовано хранилище (слой DAL).
/// </summary>
public class StudentService
{
    private readonly IRepository<Student> _students;
    private readonly IRepository<Group> _groups;

    public StudentService(IRepository<Student> students, IRepository<Group> groups)
    {
        _students = students ?? throw new ArgumentNullException(nameof(students));
        _groups = groups ?? throw new ArgumentNullException(nameof(groups));
    }

    /// <summary>Возвращает список всех студентов.</summary>
    public IReadOnlyList<Student> GetAll() => _students.GetAll();

    /// <summary>Находит студента по идентификатору.</summary>
    public Student? GetById(int id) => _students.GetById(id);

    /// <summary>Возвращает студентов указанной группы.</summary>
    public IReadOnlyList<Student> GetByGroup(int groupId) =>
        _students.Find(s => s.GroupId == groupId);

    /// <summary>Ищет студентов по началу фамилии (без учёта регистра).</summary>
    public IReadOnlyList<Student> SearchByLastName(string lastNamePrefix) =>
        _students.Find(s => s.LastName.StartsWith(lastNamePrefix, StringComparison.OrdinalIgnoreCase));

    /// <summary>Зачисляет нового студента в группу с проверкой бизнес-правил.</summary>
    public Student Enroll(string lastName, string firstName, string? middleName,
        string email, DateTime birthDate, int groupId)
    {
        ValidatePerson(lastName, firstName, email);
        var group = _groups.GetById(groupId)
            ?? throw new InvalidOperationException($"Группа с Id={groupId} не найдена");

        var numberInGroup = _students.Find(s => s.GroupId == groupId).Count + 1;
        var student = new Student
        {
            LastName = lastName.Trim(),
            FirstName = firstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            Email = email.Trim(),
            BirthDate = birthDate,
            RecordBookNumber = $"{group.Name}-{numberInGroup:D3}",
            EnrolledDate = DateTime.Today,
            Status = StudentStatus.Active,
            GroupId = groupId
        };
        return _students.Add(student);
    }

    /// <summary>Переводит студента в другую группу.</summary>
    public bool Transfer(int studentId, int targetGroupId)
    {
        var student = _students.GetById(studentId);
        var group = _groups.GetById(targetGroupId);
        if (student is null || group is null) return false;

        student.GroupId = group.Id;
        student.RecordBookNumber = GenerateRecordBookNumber(student, group);
        return _students.Update(student);
    }

    /// <summary>Отчисляет студента (переводит в статус Expelled).</summary>
    public bool Expel(int studentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Необходимо указать причину отчисления", nameof(reason));
        var student = _students.GetById(studentId);
        if (student is null || student.Status == StudentStatus.Expelled) return false;

        student.Status = StudentStatus.Expelled;
        return _students.Update(student);
    }

    private static void ValidatePerson(string lastName, string firstName, string email)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия не может быть пустой", nameof(lastName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Имя не может быть пустым", nameof(firstName));
        if (!email.Contains('@'))
            throw new ArgumentException("Некорректный адрес электронной почты", nameof(email));
    }

    private string GenerateRecordBookNumber(Student student, Group group) =>
        $"{group.Name}-{_students.Find(s => s.GroupId == group.Id).Count:D3}";
}

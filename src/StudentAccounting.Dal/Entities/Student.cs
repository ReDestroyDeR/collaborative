namespace StudentAccounting.Dal.Entities;

/// <summary>Студент — основной объект учёта.</summary>
public class Student
{
    public int Id { get; set; }
    /// <summary>Фамилия.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Имя.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Отчество (может отсутствовать).</summary>
    public string? MiddleName { get; set; }
    /// <summary>Дата рождения.</summary>
    public DateTime BirthDate { get; set; }
    /// <summary>Номер студенческого билета.</summary>
    public string RecordBookNumber { get; set; } = string.Empty;
    /// <summary>Электронная почта студента.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Телефон.</summary>
    public string? Phone { get; set; }
    /// <summary>Дата зачисления.</summary>
    public DateTime EnrolledDate { get; set; }
    /// <summary>Статус: учится / академический отпуск / отчислен.</summary>
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public int GroupId { get; set; }
    public Group? Group { get; set; }
    public List<Grade> Grades { get; set; } = new();
    public List<Attendance> Attendances { get; set; } = new();
}

/// <summary>Статус студента.</summary>
public enum StudentStatus
{
    Active,
    AcademicLeave,
    Expelled,
    Graduated
}

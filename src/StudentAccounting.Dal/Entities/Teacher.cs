namespace StudentAccounting.Dal.Entities;

/// <summary>Преподаватель.</summary>
public class Teacher : IEntity
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    /// <summary>Учёная степень и должность.</summary>
    public string Position { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
}

namespace StudentAccounting.Dal.Entities;

/// <summary>Учебная группа студентов.</summary>
public class Group : IEntity
{
    public int Id { get; set; }
    /// <summary>Номер группы, например ИТ-21.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Год формирования группы (год поступления).</summary>
    public int CreatedYear { get; set; }
    public int FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    public List<Student> Students { get; set; } = new();
}

namespace StudentAccounting.Dal.Entities;

/// <summary>Кафедра — учебно-научное подразделение факультета.</summary>
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Ссылка на факультет, к которому относится кафедра.</summary>
    public int FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    public List<Teacher> Teachers { get; set; } = new();
}

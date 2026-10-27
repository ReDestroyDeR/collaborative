namespace StudentAccounting.Dal.Entities;

/// <summary>Факультет — структурное подразделение вуза.</summary>
public class Faculty : IEntity
{
    public int Id { get; set; }
    /// <summary>Полное наименование факультета.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Краткое наименование (аббревиатура), например ИТ.</summary>
    public string ShortName { get; set; } = string.Empty;
    /// <summary>Кафедры, входящие в состав факультета.</summary>
    public List<Department> Departments { get; set; } = new();
}

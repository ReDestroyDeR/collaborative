namespace StudentAccounting.Dal.Entities;

/// <summary>Дисциплина (учебный предмет).</summary>
public class Discipline : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Общее количество часов по дисциплине.</summary>
    public int TotalHours { get; set; }
    /// <summary>Форма итоговой аттестации: экзамен или зачёт.</summary>
    public AssessmentType AssessmentType { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
}

/// <summary>Форма аттестации по дисциплине.</summary>
public enum AssessmentType
{
    Credit,
    Exam
}

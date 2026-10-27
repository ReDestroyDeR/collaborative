namespace StudentAccounting.Dal.Entities;

/// <summary>Строка учебного плана: какая дисциплина читается какой группе.</summary>
public class Curriculum
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Group? Group { get; set; }
    public int DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }
    /// <summary>Номер семестра, в котором изучается дисциплина.</summary>
    public int Semester { get; set; }
    /// <summary>Количество часов аудиторных занятий.</summary>
    public int Hours { get; set; }
}

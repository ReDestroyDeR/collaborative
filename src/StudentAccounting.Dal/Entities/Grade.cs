namespace StudentAccounting.Dal.Entities;

/// <summary>Оценка (результат аттестации) студента по дисциплине.</summary>
public class Grade : IEntity
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }
    /// <summary>Балл: 2–5 для экзамена, Зачёт/Незачёт для зачёта.</summary>
    public int Value { get; set; }
    /// <summary>Семестр, в котором получена оценка.</summary>
    public int Semester { get; set; }
    public DateTime Date { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}

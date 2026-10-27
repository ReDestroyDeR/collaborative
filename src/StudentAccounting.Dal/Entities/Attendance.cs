namespace StudentAccounting.Dal.Entities;

/// <summary>Отметка о посещаемости студента на занятии.</summary>
public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    /// <summary>Был ли студент на занятии.</summary>
    public bool IsPresent { get; set; }
    /// <summary>Причина отсутствия (болезнь, уважительная причина).</summary>
    public string? Reason { get; set; }
}

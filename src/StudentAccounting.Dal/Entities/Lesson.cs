namespace StudentAccounting.Dal.Entities;

/// <summary>Занятие расписания (пара).</summary>
public class Lesson
{
    public int Id { get; set; }
    public int DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }
    public int GroupId { get; set; }
    public Group? Group { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    /// <summary>Дата и время начала занятия.</summary>
    public DateTime StartTime { get; set; }
    /// <summary>Продолжительность в академических часах.</summary>
    public int DurationHours { get; set; }
    /// <summary>Аудитория.</summary>
    public string Room { get; set; } = string.Empty;
    public List<Attendance> Attendances { get; set; } = new();
}

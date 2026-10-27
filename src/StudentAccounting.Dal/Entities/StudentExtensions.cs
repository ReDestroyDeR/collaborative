namespace StudentAccounting.Dal.Entities;

/// <summary>Вспомогательные методы отображения сущностей.</summary>
public static class StudentExtensions
{
    /// <summary>Полное имя в формате «Фамилия Имя Отчество».</summary>
    public static string FullName(this Student s) =>
        string.IsNullOrEmpty(s.MiddleName)
            ? $"{s.LastName} {s.FirstName}"
            : $"{s.LastName} {s.FirstName} {s.MiddleName}";
}

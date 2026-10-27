namespace StudentAccounting.Bll.Scholarship;

/// <summary>Экспорт готового отчёта в текстовое представление.</summary>
public interface IReportExporter
{
    /// <summary>Название формата (CSV, Markdown, ...).</summary>
    string FormatName { get; }

    /// <summary>Формирует текстовое представление отчёта.</summary>
    string Export(ScholarshipReport report);
}

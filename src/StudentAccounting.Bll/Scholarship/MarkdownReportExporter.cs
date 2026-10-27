using System.Globalization;
using System.Text;

namespace StudentAccounting.Bll.Scholarship;

/// <summary>Экспорт отчёта в формате Markdown-таблицы.</summary>
public class MarkdownReportExporter : IReportExporter
{
    public string FormatName => "Markdown";

    /// <inheritdoc />
    public string Export(ScholarshipReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine($"## Стипендии, группа {report.GroupName}");
        sb.AppendLine();
        sb.AppendLine("| Студент | Средний балл | Сумма, руб. | Ранг |");
        sb.AppendLine("|---|---|---|---|");

        if (report.Rows.Count == 0)
        {
            sb.AppendLine("| _нет данных_ | | | |");
        }

        foreach (var row in report.Rows)
        {
            sb.Append("| ").Append(row.FullName)
              .Append(" | ").Append(row.AverageGrade.ToString("F2", CultureInfo.InvariantCulture))
              .Append(" | ").Append(row.Amount.ToString("F2", CultureInfo.InvariantCulture))
              .Append(" | ").Append(row.Rank).AppendLine(" |");
        }

        sb.AppendLine();
        sb.AppendLine($"**Итого к начислению:** {report.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)} руб.");
        return sb.ToString();
    }
}

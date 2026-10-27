using System.Globalization;
using System.Text;

namespace StudentAccounting.Bll.Scholarship;

/// <summary>Экспорт отчёта в формате CSV (разделитель «;»).</summary>
public class CsvReportExporter : IReportExporter
{
    public string FormatName => "CSV";

    /// <inheritdoc />
    public string Export(ScholarshipReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine("Группа;Фамилия Имя;Средний балл;Коэффициент;Сумма;Ранг");

        if (report.Rows.Count == 0)
        {
            sb.AppendLine($"{report.GroupName};;0.00;0;;нет данных");
            sb.AppendLine($"ИТОГО;;;;0.00;");
            return sb.ToString();
        }

        foreach (var row in report.Rows)
        {
            sb.Append(report.GroupName).Append(';')
              .Append(row.FullName).Append(';')
              .Append(row.AverageGrade.ToString("F2", CultureInfo.InvariantCulture)).Append(';')
              .Append(row.Coefficient.ToString(CultureInfo.InvariantCulture)).Append(';')
              .Append(row.Amount.ToString("F2", CultureInfo.InvariantCulture)).Append(';')
              .AppendLine(row.Rank);
        }

        sb.Append("ИТОГО;;;;")
          .Append(report.TotalAmount.ToString("F2", CultureInfo.InvariantCulture))
          .Append(';');
        return sb.ToString();
    }
}

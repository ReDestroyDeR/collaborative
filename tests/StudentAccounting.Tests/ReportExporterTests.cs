using System.Globalization;
using StudentAccounting.Bll.Scholarship;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>Тесты экспорта отчёта в формате CSV.</summary>
public class CsvReportExporterTests
{
    private readonly CsvReportExporter _exporter = new();

    private static ScholarshipReport MakeReport(int rowsCount)
    {
        var report = new ScholarshipReport { GroupId = 1, GroupName = "ИТ-21" };
        if (rowsCount > 0)
        {
            report.Rows.Add(new ScholarshipReportRow
            {
                StudentId = 10, FullName = "Иванов Пётр", AverageGrade = 4.5,
                Coefficient = 1.5m, Amount = 3000m, Rank = "отличник"
            });
        }
        if (rowsCount > 1)
        {
            report.Rows.Add(new ScholarshipReportRow
            {
                StudentId = 11, FullName = "Петрова Анна", AverageGrade = 4.0,
                Coefficient = 1.0m, Amount = 2000m, Rank = "хорошист"
            });
            report.TotalAmount = 5000m;
        }
        return report;
    }

    [Fact]
    public void Export_NullReport_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => _exporter.Export(null!));

    [Fact]
    public void Export_EmptyReport_ContainsNoDataMarker()
    {
        var text = _exporter.Export(MakeReport(0));

        Assert.Contains("нет данных", text);
        Assert.Contains("ИТОГО", text);
    }

    [Fact]
    public void Export_ReportWithRows_WritesHeaderAndOneLinePerRow()
    {
        var text = _exporter.Export(MakeReport(2));
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.StartsWith("Группа;Фамилия Имя;", lines[0]);
        Assert.Equal(4, lines.Length);                       // заголовок + 2 строки + ИТОГО
        Assert.Contains("Иванов Пётр;4.50;1.5;3000.00;отличник", text, StringComparison.Ordinal);
        Assert.Contains("Петрова Анна;4.00;1.0;2000.00;хорошист", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_ReportWithRows_WritesTotalLine()
    {
        var text = _exporter.Export(MakeReport(2));

        Assert.Contains($"ИТОГО;;;;{5000m.ToString("F2", CultureInfo.InvariantCulture)};", text);
    }

    [Fact]
    public void FormatName_IsCsv() => Assert.Equal("CSV", _exporter.FormatName);
}

/// <summary>Тесты экспорта отчёта в формате Markdown.</summary>
public class MarkdownReportExporterTests
{
    private readonly MarkdownReportExporter _exporter = new();

    [Fact]
    public void Export_NullReport_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => _exporter.Export(null!));

    [Fact]
    public void Export_EmptyReport_ContainsNoDataMarker()
    {
        var text = _exporter.Export(new ScholarshipReport { GroupId = 2, GroupName = "ИТ-22" });

        Assert.Contains("нет данных", text);
        Assert.Contains("**Итого к начислению:** 0.00", text);
    }

    [Fact]
    public void Export_ReportWithRows_WritesTableAndTotal()
    {
        var report = new ScholarshipReport { GroupId = 1, GroupName = "ИТ-21" };
        report.Rows.Add(new ScholarshipReportRow
        {
            StudentId = 10, FullName = "Иванов Пётр", AverageGrade = 4.5,
            Coefficient = 1.5m, Amount = 3000m, Rank = "отличник"
        });
        report.TotalAmount = 3000m;

        var text = _exporter.Export(report);

        Assert.Contains("## Стипендии, группа ИТ-21", text);
        Assert.Contains("| Иванов Пётр | 4.50 | 3000.00 | отличник |", text);
        Assert.Contains("**Итого к начислению:** 3000.00", text);
    }

    [Fact]
    public void FormatName_IsMarkdown() => Assert.Equal("Markdown", _exporter.FormatName);
}

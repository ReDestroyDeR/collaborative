namespace StudentAccounting.Bll.Scholarship;

/// <summary>Строка отчёта по одному студенту.</summary>
public class ScholarshipReportRow
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public double AverageGrade { get; init; }
    /// <summary>Коэффициент стипендии, применённый политикой.</summary>
    public decimal Coefficient { get; init; }
    /// <summary>Начисленная сумма, руб.</summary>
    public decimal Amount { get; init; }
    /// <summary>Учебный ранг: отличник / хорошист / нет оценок.</summary>
    public string Rank { get; init; } = string.Empty;
}

/// <summary>Отчёт о назначении стипендий по группе за расчётный период.</summary>
public class ScholarshipReport
{
    public int GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<ScholarshipReportRow> Rows { get; init; } = new();

    /// <summary>Итоговая сумма начислений по группе, руб.</summary>
    public decimal TotalAmount { get; set; }
}

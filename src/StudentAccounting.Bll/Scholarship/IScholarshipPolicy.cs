namespace StudentAccounting.Bll.Scholarship;

/// <summary>
/// Политика (правила) назначения стипендии.
/// Выделена в интерфейс, чтобы правила можно было менять независимо
/// от остальной логики и подменять в модульных тестах.
/// </summary>
public interface IScholarshipPolicy
{
    /// <summary>Базовый размер стипендии, руб.</summary>
    decimal BaseAmount { get; }

    /// <summary>
    /// Возвращает коэффициент (долю) базовой стипендии для указанного среднего балла.
    /// Например: 0 — стипендия не назначается, 1.0 — обычная, 1.5 — повышенная.
    /// </summary>
    decimal GetCoefficient(double averageGrade);
}

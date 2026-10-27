namespace StudentAccounting.Bll.Scholarship;

/// <summary>
/// Стандартные правила назначения стипендии:
///   средний балл 4.5 и выше — повышенная стипендия (коэффициент 1.5);
///   средний балл от 3.5 до 4.5 — обычная стипендия (коэффициент 1.0);
///   ниже 3.5 или оценок нет — стипендия не назначается (коэффициент 0).
/// </summary>
public class StandardScholarshipPolicy : IScholarshipPolicy
{
    public const decimal PremiumCoefficient = 1.5m;
    public const decimal StandardCoefficient = 1.0m;
    public const decimal NoneCoefficient = 0m;

    public const double ExcellentThreshold = 4.5;
    public const double GoodThreshold = 3.5;

    public decimal BaseAmount => 2000m;

    /// <inheritdoc />
    public decimal GetCoefficient(double averageGrade)
    {
        if (double.IsNaN(averageGrade) || averageGrade < 0)
            throw new ArgumentOutOfRangeException(nameof(averageGrade),
                averageGrade, "Средний балл не может быть отрицательным или не числом");
        if (averageGrade == 0)
            return NoneCoefficient;
        if (averageGrade >= ExcellentThreshold)
            return PremiumCoefficient;
        if (averageGrade >= GoodThreshold)
            return StandardCoefficient;
        return NoneCoefficient;
    }
}

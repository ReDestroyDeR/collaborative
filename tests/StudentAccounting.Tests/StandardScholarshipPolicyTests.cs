using StudentAccounting.Bll.Scholarship;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>Модульные тесты стандартной политики назначения стипендии.</summary>
public class StandardScholarshipPolicyTests
{
    private readonly StandardScholarshipPolicy _policy = new();

    [Theory]
    [InlineData(4.5, 1.5)]
    [InlineData(4.99, 1.5)]
    [InlineData(5.0, 1.5)]
    public void GetCoefficient_ExcellentAverage_ReturnsPremiumCoefficient(double average, decimal expected) =>
        Assert.Equal(expected, _policy.GetCoefficient(average));

    [Theory]
    [InlineData(3.5, 1.0)]
    [InlineData(4.0, 1.0)]
    [InlineData(4.49, 1.0)]
    public void GetCoefficient_GoodAverage_ReturnsStandardCoefficient(double average, decimal expected) =>
        Assert.Equal(expected, _policy.GetCoefficient(average));

    [Theory]
    [InlineData(3.49, 0)]
    [InlineData(2.0, 0)]
    public void GetCoefficient_LowAverage_ReturnsZeroCoefficient(double average, decimal expected) =>
        Assert.Equal(expected, _policy.GetCoefficient(average));

    [Fact]
    public void GetCoefficient_ZeroAverage_NoGrades_ReturnsZeroCoefficient() =>
        Assert.Equal(0m, _policy.GetCoefficient(0));

    [Fact]
    public void GetCoefficient_NegativeAverage_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _policy.GetCoefficient(-0.1));

    [Fact]
    public void GetCoefficient_NaN_ThrowsArgumentOutOfRangeException() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _policy.GetCoefficient(double.NaN));

    [Fact]
    public void BaseAmount_EqualsTwoThousandRubles() =>
        Assert.Equal(2000m, _policy.BaseAmount);
}

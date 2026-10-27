using Moq;
using StudentAccounting.Bll.Scholarship;
using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>
/// Модульные тесты сервиса расчёта стипендий.
/// Все зависимости подменяются mock-объектами (Moq), тестируется только логика сервиса.
/// </summary>
public class ScholarshipServiceTests
{
    private readonly Mock<IRepository<Student>> _students = new();
    private readonly Mock<IRepository<Group>> _groups = new();
    private readonly Mock<IRepository<Grade>> _grades = new();
    private readonly Mock<IScholarshipPolicy> _policy = new();
    private readonly Mock<IReportExporter> _exporter = new();

    private ScholarshipService CreateService() =>
        new(_students.Object, _groups.Object, _grades.Object, _policy.Object, _exporter.Object);

    public ScholarshipServiceTests() =>
        _policy.Setup(p => p.BaseAmount).Returns(2000m);

    private static Student MakeStudent(int id, string lastName, string firstName, int groupId) =>
        new() { Id = id, LastName = lastName, FirstName = firstName, GroupId = groupId };

    private static Grade MakeGrade(int studentId, int value) =>
        new() { StudentId = studentId, Value = value };

    // ---------- построение отчёта ----------

    [Fact]
    public void BuildReport_GroupNotFound_ThrowsInvalidOperationException()
    {
        _groups.Setup(g => g.GetById(99)).Returns((Group?)null);

        var service = CreateService();

        var exception = Assert.Throws<InvalidOperationException>(() => _service_BuildReport(service, 99));
        Assert.Contains("99", exception.Message);
    }

    private static ScholarshipReport _service_BuildReport(ScholarshipService service, int groupId) =>
        service.BuildReport(groupId);

    [Fact]
    public void BuildReport_StudentWithGrades_CalculatesAverageAndAmount()
    {
        var group = new Group { Id = 1, Name = "ИТ-21" };
        _groups.Setup(g => g.GetById(1)).Returns(group);
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>()))
            .Returns(new List<Student> { MakeStudent(10, "Иванов", "Пётр", 1) });
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns(new List<Grade> { MakeGrade(10, 5), MakeGrade(10, 4) });
        _policy.Setup(p => p.GetCoefficient(4.5)).Returns(1.5m);

        var report = CreateService().BuildReport(1);

        var row = Assert.Single(report.Rows);
        Assert.Equal(4.5, row.AverageGrade);
        Assert.Equal(1.5m, row.Coefficient);
        Assert.Equal(3000m, row.Amount);
        Assert.Equal("отличник", row.Rank);
        Assert.Equal(3000m, report.TotalAmount);
        _policy.Verify(p => p.GetCoefficient(4.5), Times.Once);
    }

    [Fact]
    public void BuildReport_StudentWithoutGrades_ZeroAverageAndAmount()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>()))
            .Returns(new List<Student> { MakeStudent(11, "Петрова", "Анна", 1) });
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());
        _policy.Setup(p => p.GetCoefficient(0)).Returns(0m);

        var report = CreateService().BuildReport(1);

        var row = Assert.Single(report.Rows);
        Assert.Equal(0, row.AverageGrade);
        Assert.Equal(0m, row.Amount);
        Assert.Equal("нет оценок", row.Rank);
        Assert.Equal(0m, report.TotalAmount);
    }

    [Fact]
    public void BuildReport_SeveralStudents_SumsTotalAmount()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>
        {
            MakeStudent(10, "Иванов", "Пётр", 1),
            MakeStudent(11, "Петрова", "Анна", 1)
        });
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns((Func<Grade, bool> predicate) => new List<Grade>
            {
                MakeGrade(10, 5), MakeGrade(10, 4), MakeGrade(11, 4), MakeGrade(11, 4)
            }.Where(predicate).ToList());
        _policy.Setup(p => p.GetCoefficient(It.IsAny<double>()))
            .Returns<double>(avg => avg >= 4.5 ? 1.5m : 1.0m);

        var report = CreateService().BuildReport(1);

        Assert.Equal(2, report.Rows.Count);
        Assert.Equal(5000m, report.TotalAmount);
    }

    [Fact]
    public void BuildReport_SortsStudentsByLastName()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>
        {
            MakeStudent(20, "Смирнов", "Олег", 1),
            MakeStudent(21, "Иванов", "Пётр", 1)
        });
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());
        _policy.Setup(p => p.GetCoefficient(It.IsAny<double>())).Returns(0m);

        var report = CreateService().BuildReport(1);

        Assert.Equal(new[] { "Иванов Пётр", "Смирнов Олег" }, report.Rows.Select(r => r.FullName).ToArray());
    }

    [Fact]
    public void BuildReport_StudentsOfOtherGroups_AreNotIncluded()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>()))
            .Returns<Func<Student, bool>>(predicate => new List<Student>
            {
                MakeStudent(30, "Иванов", "Пётр", 1),
                MakeStudent(31, "Сидоров", "Олег", 2)
            }.Where(predicate).ToList());
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns(new List<Grade> { MakeGrade(30, 4) });
        _policy.Setup(p => p.GetCoefficient(4.0)).Returns(1.0m);

        var report = CreateService().BuildReport(1);

        var row = Assert.Single(report.Rows);
        Assert.Equal("Иванов Пётр", row.FullName);
    }

    // ---------- экспорт ----------

    [Fact]
    public void Export_NullReport_ThrowsArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => CreateService().Export(null!));

    [Fact]
    public void Export_ValidReport_DelegatesToExporter()
    {
        var report = new ScholarshipReport { GroupId = 1, GroupName = "ИТ-21" };
        _exporter.Setup(e => e.Export(report)).Returns("CSV-DATA");

        var result = CreateService().Export(report);

        Assert.Equal("CSV-DATA", result);
        _exporter.Verify(e => e.Export(report), Times.Once);
    }

    // ---------- безопасное построение отчёта ----------

    [Fact]
    public void TryBuildReport_ValidGroup_ReturnsTrueAndReport()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>());
        _grades.Setup(g => g.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());

        var ok = CreateService().TryBuildReport(1, out var report, out var error);

        Assert.True(ok);
        Assert.NotNull(report);
        Assert.Equal("ИТ-21", report!.GroupName);
        Assert.Null(error);
    }

    [Fact]
    public void TryBuildReport_GroupNotFound_ReturnsFalseWithError()
    {
        _groups.Setup(g => g.GetById(99)).Returns((Group?)null);

        var ok = CreateService().TryBuildReport(99, out var report, out var error);

        Assert.False(ok);
        Assert.Null(report);
        Assert.Equal("Группа с Id=99 не найдена", error);
    }

    [Fact]
    public void TryBuildReport_UnexpectedException_IsNotSwallowed()
    {
        _groups.Setup(g => g.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        _students.Setup(s => s.Find(It.IsAny<Func<Student, bool>>()))
            .Throws(new ApplicationException("сбой хранилища"));

        Assert.Throws<ApplicationException>(() => CreateService().TryBuildReport(1, out _, out _));
    }

    // ---------- учебный ранг ----------

    [Theory]
    [InlineData(0, false, "нет оценок")]
    [InlineData(4.9, true, "отличник")]
    [InlineData(4.0, true, "хорошист")]
    [InlineData(2.5, true, "требует внимания")]
    public void GetRank_VariousAverages_ReturnsExpectedRank(double average, bool hasGrades, string expected) =>
        Assert.Equal(expected, ScholarshipService.GetRank(average, hasGrades));

    // ---------- проверка аргументов конструктора ----------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Constructor_NullDependency_ThrowsArgumentNullException(int nullArgument)
    {
        var students = nullArgument == 0 ? null! : Mock.Of<IRepository<Student>>();
        var groups = nullArgument == 1 ? null! : Mock.Of<IRepository<Group>>();
        var grades = nullArgument == 2 ? null! : Mock.Of<IRepository<Grade>>();
        var policy = nullArgument == 3 ? null! : Mock.Of<IScholarshipPolicy>();
        var exporter = nullArgument == 4 ? null! : Mock.Of<IReportExporter>();

        Assert.Throws<ArgumentNullException>(() =>
            new ScholarshipService(students, groups, grades, policy, exporter));
    }
}

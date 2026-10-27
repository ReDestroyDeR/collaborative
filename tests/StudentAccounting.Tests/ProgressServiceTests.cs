using Moq;
using StudentAccounting.Bll.Services;
using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>Модульные тесты сервиса успеваемости и посещаемости.</summary>
public class ProgressServiceTests
{
    private readonly Mock<IRepository<Student>> _students = new();
    private readonly Mock<IRepository<Grade>> _grades = new();
    private readonly Mock<IRepository<Attendance>> _attendance = new();

    private ProgressService CreateService() =>
        new(_students.Object, _grades.Object, _attendance.Object);

    public ProgressServiceTests()
    {
        // значения по умолчанию: пустые выборки
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>());
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());
        _attendance.Setup(r => r.Find(It.IsAny<Func<Attendance, bool>>())).Returns(new List<Attendance>());
    }

    private static Grade MakeGrade(int studentId, int value, int semester = 1) =>
        new() { StudentId = studentId, Value = value, Semester = semester };

    private static Attendance MakeAttendance(int studentId, bool present) =>
        new() { StudentId = studentId, IsPresent = present };

    // ---------- средний балл студента ----------

    [Fact]
    public void GetAverageGrade_NoGrades_ReturnsZero()
    {
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());

        Assert.Equal(0, CreateService().GetAverageGrade(1));
    }

    [Fact]
    public void GetAverageGrade_WithGrades_ReturnsAverage()
    {
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns<Func<Grade, bool>>(p => new List<Grade> { MakeGrade(1, 5), MakeGrade(1, 4) }.Where(p).ToList());

        Assert.Equal(4.5, CreateService().GetAverageGrade(1));
    }

    // ---------- средний балл группы ----------

    [Fact]
    public void GetGroupAverageGrade_NoStudentsInGroup_ReturnsZero()
    {
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>());

        Assert.Equal(0, CreateService().GetGroupAverageGrade(1));
    }

    [Fact]
    public void GetGroupAverageGrade_StudentsWithoutGrades_ReturnsZero()
    {
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>()))
            .Returns<Func<Student, bool>>(p => new List<Student>
            {
                new() { Id = 1, GroupId = 1 }, new() { Id = 2, GroupId = 1 }
            }.Where(p).ToList());
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>())).Returns(new List<Grade>());

        Assert.Equal(0, CreateService().GetGroupAverageGrade(1));
    }

    [Fact]
    public void GetGroupAverageGrade_GradesOfOtherStudents_AreIgnored()
    {
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>()))
            .Returns<Func<Student, bool>>(p => new List<Student>
            {
                new() { Id = 1, GroupId = 1 }, new() { Id = 2, GroupId = 1 }, new() { Id = 3, GroupId = 2 }
            }.Where(p).ToList());
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns<Func<Grade, bool>>(p => new List<Grade>
            {
                MakeGrade(1, 5), MakeGrade(1, 4), MakeGrade(3, 2)   // оценка студента 3 — другая группа
            }.Where(p).ToList());

        Assert.Equal(4.5, CreateService().GetGroupAverageGrade(1));
    }

    // ---------- посещаемость ----------

    [Fact]
    public void GetAttendanceRate_NoRecords_ReturnsZero()
    {
        _attendance.Setup(r => r.Find(It.IsAny<Func<Attendance, bool>>())).Returns(new List<Attendance>());

        Assert.Equal(0, CreateService().GetAttendanceRate(1));
    }

    [Theory]
    [InlineData(true, false, true, 66.67)]
    [InlineData(false, false, false, 0.0)]
    public void GetAttendanceRate_MixedRecords_ReturnsPercentage(bool p1, bool p2, bool p3, double expected)
    {
        _attendance.Setup(r => r.Find(It.IsAny<Func<Attendance, bool>>()))
            .Returns<Func<Attendance, bool>>(p => new List<Attendance>
            {
                MakeAttendance(1, p1), MakeAttendance(1, p2), MakeAttendance(1, p3)
            }.Where(p).ToList());

        Assert.Equal(expected, CreateService().GetAttendanceRate(1), precision: 2);
    }

    // ---------- оценки и ранги ----------

    [Fact]
    public void GetGrades_ReturnsOrderedBySemester()
    {
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns<Func<Grade, bool>>(p => new List<Grade>
            {
                MakeGrade(1, 4, semester: 3), MakeGrade(1, 5, semester: 1)
            }.Where(p).ToList());

        var result = CreateService().GetGrades(1);

        Assert.Equal(new[] { 5, 4 }, result.Select(g => g.Value));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void IsExcellent_Average45_ReturnsTrue()
    {
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns<Func<Grade, bool>>(p => new List<Grade>
            {
                MakeGrade(1, 5), MakeGrade(1, 4)                       // средний балл 4.5
            }.Where(p).ToList());

        Assert.True(CreateService().IsExcellent(1));
    }

    [Fact]
    public void IsExcellent_AverageBelow45_ReturnsFalse()
    {
        _grades.Setup(r => r.Find(It.IsAny<Func<Grade, bool>>()))
            .Returns<Func<Grade, bool>>(p => new List<Grade>
            {
                MakeGrade(1, 4), MakeGrade(1, 4)                       // средний балл 4.0
            }.Where(p).ToList());

        Assert.False(CreateService().IsExcellent(1));
    }

    [Fact]
    public void IsExcellent_NoGrades_ReturnsFalse()
    {
        Assert.False(CreateService().IsExcellent(1));
    }
}

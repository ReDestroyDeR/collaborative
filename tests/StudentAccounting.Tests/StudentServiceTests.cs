using Moq;
using StudentAccounting.Bll.Services;
using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>Модульные тесты сервиса работы со студентами (зависимости — Moq).</summary>
public class StudentServiceTests
{
    private readonly Mock<IRepository<Student>> _students = new();
    private readonly Mock<IRepository<Group>> _groups = new();

    private StudentService CreateService() => new(_students.Object, _groups.Object);

    public StudentServiceTests()
    {
        // значения по умолчанию: пустые выборки, Add возвращает добавленную сущность
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>())).Returns(new List<Student>());
        _students.Setup(r => r.Add(It.IsAny<Student>())).Returns<Student>(s => s);
    }

    private void SetupStudents(params Student[] students) =>
        _students.Setup(r => r.Find(It.IsAny<Func<Student, bool>>()))
            .Returns<Func<Student, bool>>(predicate => students.Where(predicate).ToList());

    private static Student MakeStudent(int id, string lastName = "Иванов", int groupId = 1,
        StudentStatus status = StudentStatus.Active, string recordBook = "ИТ-21-001") =>
        new() { Id = id, LastName = lastName, FirstName = "Пётр", GroupId = groupId, Status = status, RecordBookNumber = recordBook };

    // ---------- чтение ----------

    [Fact]
    public void GetAll_ReturnsAllStudentsFromRepository()
    {
        var expected = new List<Student> { MakeStudent(1), MakeStudent(2, "Петров") };
        _students.Setup(r => r.GetAll()).Returns(expected);

        var result = CreateService().GetAll();

        Assert.Same(expected, result);
    }

    [Fact]
    public void GetById_Found_ReturnsStudent()
    {
        var student = MakeStudent(7, "Смирнов");
        _students.Setup(r => r.GetById(7)).Returns(student);

        var result = CreateService().GetById(7);

        Assert.Same(student, result);
    }

    [Fact]
    public void GetById_Missing_ReturnsNull()
    {
        _students.Setup(r => r.GetById(7)).Returns((Student?)null);

        Assert.Null(CreateService().GetById(7));
    }

    [Theory]
    [InlineData("Ив", 1)]
    [InlineData("Сидоров", 0)]
    public void SearchByLastName_Prefix_ReturnsOnlyMatchingStudents(string prefix, int expectedCount)
    {
        SetupStudents(MakeStudent(1, "Иванов"), MakeStudent(2, "Петров"));

        var result = CreateService().SearchByLastName(prefix);

        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetByGroup_ReturnsOnlyStudentsOfGroup()
    {
        SetupStudents(MakeStudent(1, "Иванов", groupId: 1), MakeStudent(2, "Сидоров", groupId: 2));

        var result = CreateService().GetByGroup(1);

        Assert.Equal(new[] { 1 }, result.Select(s => s.Id));
    }

    // ---------- зачисление ----------

    [Fact]
    public void Enroll_ValidStudent_AddsStudentAndGeneratesRecordBookNumber()
    {
        _groups.Setup(r => r.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        SetupStudents(MakeStudent(1, "Иванов", recordBook: "ИТ-21-001"));
        Student? added = null;
        _students.Setup(r => r.Add(It.IsAny<Student>()))
            .Callback<Student>(s => added = s)
            .Returns<Student>(s => s);

        var result = CreateService().Enroll("Новиков", "Игорь", "Анатольевич",
            "novikov@university.ru", new DateTime(2005, 5, 17), 1);

        Assert.NotNull(added);
        Assert.Equal("ИТ-21-002", result.RecordBookNumber);
        Assert.Equal(StudentStatus.Active, result.Status);
        Assert.Equal(1, result.GroupId);
        Assert.Equal("Анатольевич", result.MiddleName);
        _students.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public void Enroll_WithoutMiddleName_MiddleNameIsNull()
    {
        _groups.Setup(r => r.GetById(1)).Returns(new Group { Id = 1, Name = "ИТ-21" });
        SetupStudents();

        var result = CreateService().Enroll("Новиков", "Игорь", null, "n@university.ru", new DateTime(2005, 5, 17), 1);

        Assert.Null(result.MiddleName);
    }

    [Theory]
    [InlineData("", "Игорь", "n@university.ru", "Фамилия")]
    [InlineData(null, "Игорь", "n@university.ru", "Фамилия")]
    [InlineData("Новиков", "", "n@university.ru", "Имя")]
    [InlineData("Новиков", "Игорь", "некорректный-адрес", "почты")]
    public void Enroll_InvalidArgument_ThrowsArgumentException(string? lastName, string firstName, string email, string expectedPart)
    {
        SetupStudents();

        var exception = Assert.Throws<ArgumentException>(() =>
            CreateService().Enroll(lastName!, firstName, null, email, new DateTime(2005, 1, 1), 1));

        Assert.Contains(expectedPart, exception.Message);
    }

    [Fact]
    public void Enroll_GroupNotFound_ThrowsInvalidOperationException()
    {
        SetupStudents();
        _groups.Setup(r => r.GetById(99)).Returns((Group?)null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CreateService().Enroll("Новиков", "Игорь", null, "n@university.ru", new DateTime(2005, 1, 1), 99));

        Assert.Contains("99", exception.Message);
    }

    // ---------- перевод ----------

    [Fact]
    public void Transfer_StudentNotFound_ReturnsFalse()
    {
        _students.Setup(r => r.GetById(1)).Returns((Student?)null);
        _groups.Setup(r => r.GetById(2)).Returns(new Group { Id = 2, Name = "ИТ-22" });

        Assert.False(CreateService().Transfer(1, 2));
    }

    [Fact]
    public void Transfer_GroupNotFound_ReturnsFalse()
    {
        _students.Setup(r => r.GetById(1)).Returns(MakeStudent(1));
        _groups.Setup(r => r.GetById(99)).Returns((Group?)null);

        Assert.False(CreateService().Transfer(1, 99));
    }

    [Fact]
    public void Transfer_Success_ChangesGroupAndRecordBookNumber()
    {
        var student = MakeStudent(1);
        _students.Setup(r => r.GetById(1)).Returns(student);
        _groups.Setup(r => r.GetById(2)).Returns(new Group { Id = 2, Name = "ИТ-22" });
        SetupStudents(student);
        Student? updated = null;
        _students.Setup(r => r.Update(It.IsAny<Student>()))
            .Callback<Student>(s => updated = s)
            .Returns(true);

        var result = CreateService().Transfer(1, 2);

        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal(2, updated!.GroupId);
        Assert.Equal("ИТ-22-001", updated.RecordBookNumber);
    }

    // ---------- отчисление ----------

    [Fact]
    public void Expel_EmptyReason_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateService().Expel(1, "  "));
    }

    [Fact]
    public void Expel_StudentNotFound_ReturnsFalse()
    {
        _students.Setup(r => r.GetById(1)).Returns((Student?)null);

        Assert.False(CreateService().Expel(1, "по собственному желанию"));
    }

    [Fact]
    public void Expel_AlreadyExpelled_ReturnsFalse()
    {
        _students.Setup(r => r.GetById(1))
            .Returns(MakeStudent(1, status: StudentStatus.Expelled));

        Assert.False(CreateService().Expel(1, "по собственному желанию"));
    }

    [Fact]
    public void Expel_ActiveStudent_ChangesStatus()
    {
        var student = MakeStudent(1);
        _students.Setup(r => r.GetById(1)).Returns(student);
        Student? updated = null;
        _students.Setup(r => r.Update(It.IsAny<Student>()))
            .Callback<Student>(s => updated = s)
            .Returns(true);

        var result = CreateService().Expel(1, "по собственному желанию");

        Assert.True(result);
        Assert.Equal(StudentStatus.Expelled, updated!.Status);
    }

    // ---------- конструктор ----------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Constructor_NullDependency_ThrowsArgumentNullException(int nullArgument)
    {
        var students = nullArgument == 0 ? null! : Mock.Of<IRepository<Student>>();
        var groups = nullArgument == 1 ? null! : Mock.Of<IRepository<Group>>();

        Assert.Throws<ArgumentNullException>(() => new StudentService(students, groups));
    }
}

/// <summary>Тесты вспомогательных методов сущностей.</summary>
public class StudentExtensionsTests
{
    [Fact]
    public void FullName_WithMiddleName_ReturnsThreeParts()
    {
        var student = new Student { LastName = "Иванов", FirstName = "Пётр", MiddleName = "Сергеевич" };

        Assert.Equal("Иванов Пётр Сергеевич", student.FullName());
    }

    [Fact]
    public void FullName_WithoutMiddleName_ReturnsTwoParts()
    {
        var student = new Student { LastName = "Иванов", FirstName = "Пётр", MiddleName = null };

        Assert.Equal("Иванов Пётр", student.FullName());
    }

    [Fact]
    public void FullName_EmptyMiddleName_ReturnsTwoParts()
    {
        var student = new Student { LastName = "Иванов", FirstName = "Пётр", MiddleName = "" };

        Assert.Equal("Иванов Пётр", student.FullName());
    }
}

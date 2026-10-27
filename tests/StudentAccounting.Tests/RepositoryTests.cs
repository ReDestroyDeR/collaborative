using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;
using Xunit;

namespace StudentAccounting.Tests;

/// <summary>Тесты репозитория «в памяти».</summary>
public class InMemoryRepositoryTests
{
    private static Student MakeStudent(int id, string lastName) =>
        new() { Id = id, LastName = lastName, FirstName = "Тест" };

    [Fact]
    public void Add_NewEntity_GeneratesId()
    {
        var repository = new InMemoryRepository<Student>();

        var added = repository.Add(MakeStudent(0, "Иванов"));

        Assert.Equal(1, added.Id);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Add_SecondEntity_IncrementsId()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(MakeStudent(0, "Иванов"));

        var added = repository.Add(MakeStudent(0, "Петров"));

        Assert.Equal(2, added.Id);
    }

    [Fact]
    public void Add_DuplicateKey_ThrowsInvalidOperationException()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(MakeStudent(5, "Иванов"));

        Assert.Throws<InvalidOperationException>(() => repository.Add(MakeStudent(5, "Петров")));
    }

    [Fact]
    public void Constructor_Seed_PreservesIds()
    {
        var repository = new InMemoryRepository<Student>(new[]
        {
            MakeStudent(7, "Иванов"), MakeStudent(9, "Петров")
        });

        Assert.NotNull(repository.GetById(7));
        Assert.NotNull(repository.GetById(9));
        Assert.Equal(2, repository.GetAll().Count);
    }

    [Fact]
    public void GetById_MissingEntity_ReturnsNull()
    {
        var repository = new InMemoryRepository<Student>();

        Assert.Null(repository.GetById(100));
    }

    [Fact]
    public void Find_Predicate_ReturnsOnlyMatching()
    {
        var repository = new InMemoryRepository<Student>(new[]
        {
            MakeStudent(1, "Иванов"), MakeStudent(2, "Петров"), MakeStudent(3, "Ильин")
        });

        var result = repository.Find(s => s.LastName.StartsWith("И", StringComparison.Ordinal));

        Assert.Equal(new[] { 1, 3 }, result.Select(s => s.Id));
    }

    [Fact]
    public void Update_ExistingEntity_ReturnsTrueAndAppliesChanges()
    {
        var repository = new InMemoryRepository<Student>(new[] { MakeStudent(1, "Иванов") });
        var changed = MakeStudent(1, "Смирнов");

        var updated = repository.Update(changed);

        Assert.True(updated);
        Assert.Equal("Смирнов", repository.GetById(1)!.LastName);
    }

    [Fact]
    public void Update_MissingEntity_ReturnsFalse() =>
        Assert.False(new InMemoryRepository<Student>().Update(MakeStudent(42, "Иванов")));

    [Fact]
    public void Delete_ExistingEntity_ReturnsTrueAndRemoves()
    {
        var repository = new InMemoryRepository<Student>(new[] { MakeStudent(1, "Иванов") });

        Assert.True(repository.Delete(1));
        Assert.Null(repository.GetById(1));
    }

    [Fact]
    public void Delete_MissingEntity_ReturnsFalse() =>
        Assert.False(new InMemoryRepository<Student>().Delete(1));
}

/// <summary>Тесты JSON-репозитория (System.Text.Json).</summary>
public class JsonRepositoryTests : IDisposable
{
    private readonly string _filePath =
        Path.Combine(Path.GetTempPath(), $"students_test_{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }

    private static Student MakeStudent(int id, string lastName) =>
        new() { Id = id, LastName = lastName, FirstName = "Тест", RecordBookNumber = $"№{id}" };

    [Fact]
    public void SaveThenLoad_RoundTrip_RestoresEntities()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));
        repository.Add(MakeStudent(2, "Петрова"));
        repository.Save();

        var reloaded = new JsonRepository<Student>(_filePath);

        Assert.Equal(2, reloaded.GetAll().Count);
        Assert.Equal("Иванов", reloaded.GetById(1)!.LastName);
        Assert.Equal("Петрова", reloaded.GetById(2)!.LastName);
        Assert.Equal("№2", reloaded.GetById(2)!.RecordBookNumber);
    }

    [Fact]
    public void Constructor_MissingFile_CreatesEmptyRepository()
    {
        var repository = new JsonRepository<Student>(_filePath);

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Save_WritesNonEmptyFile()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));

        repository.Save();

        Assert.True(File.Exists(_filePath));
        Assert.True(new FileInfo(_filePath).Length > 10);
    }

    [Fact]
    public void UpdateThenSave_ChangesArePersisted()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));
        repository.Save();

        var changed = MakeStudent(1, "Смирнов");
        Assert.True(repository.Update(changed));
        repository.Save();

        var reloaded = new JsonRepository<Student>(_filePath);
        Assert.Equal("Смирнов", reloaded.GetById(1)!.LastName);
    }

    [Fact]
    public void DeleteThenSave_EntityIsRemoved()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));
        repository.Save();

        Assert.True(repository.Delete(1));
        repository.Save();

        var reloaded = new JsonRepository<Student>(_filePath);
        Assert.Empty(reloaded.GetAll());
    }

    [Fact]
    public void Add_DuplicateKey_ThrowsInvalidOperationException()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));

        Assert.Throws<InvalidOperationException>(() => repository.Add(MakeStudent(1, "Петров")));
    }
}

/// <summary>Дополнительные тесты ветвей JSON-репозитория.</summary>
public class JsonRepositoryBranchTests : IDisposable
{
    private readonly string _filePath =
        Path.Combine(Path.GetTempPath(), $"students_test_{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }

    private static Student MakeStudent(int id, string lastName) =>
        new() { Id = id, LastName = lastName, FirstName = "Тест" };

    [Fact]
    public void Add_EntityWithoutId_GeneratesNextId()
    {
        var repository = new JsonRepository<Student>(_filePath);

        var added = repository.Add(MakeStudent(0, "Иванов"));

        Assert.Equal(1, added.Id);
    }

    [Fact]
    public void Add_EntityWithoutId_WhenFileHasEntities_GeneratesNextId()
    {
        var first = new JsonRepository<Student>(_filePath);
        first.Add(MakeStudent(4, "Иванов"));
        first.Save();

        var second = new JsonRepository<Student>(_filePath);
        var added = second.Add(MakeStudent(0, "Петров"));

        Assert.Equal(5, added.Id);
    }

    [Fact]
    public void Constructor_FileWithNullContent_ReturnsEmptyRepository()
    {
        File.WriteAllText(_filePath, "null");

        var repository = new JsonRepository<Student>(_filePath);

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Update_MissingEntity_ReturnsFalse()
    {
        var repository = new JsonRepository<Student>(_filePath);

        Assert.False(repository.Update(MakeStudent(42, "Иванов")));
    }

    [Fact]
    public void Delete_MissingEntity_ReturnsFalse()
    {
        var repository = new JsonRepository<Student>(_filePath);

        Assert.False(repository.Delete(42));
    }

    [Fact]
    public void Find_Predicate_ReturnsOnlyMatching()
    {
        var repository = new JsonRepository<Student>(_filePath);
        repository.Add(MakeStudent(1, "Иванов"));
        repository.Add(MakeStudent(2, "Петров"));

        var result = repository.Find(s => s.LastName == "Петров");

        Assert.Equal(new[] { 2 }, result.Select(s => s.Id));
    }
}

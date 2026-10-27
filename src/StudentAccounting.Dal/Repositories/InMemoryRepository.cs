using StudentAccounting.Dal.Entities;

namespace StudentAccounting.Dal.Repositories;

/// <summary>
/// Реализация репозитория «в памяти» (List&lt;T&gt;).
/// Используется для демонстрации и модульных тестов.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();
    private readonly object _lock = new();

    public InMemoryRepository() { }

    /// <summary>Создаёт репозиторий и заполняет его начальными данными.</summary>
    public InMemoryRepository(IEnumerable<T> seed)
    {
        foreach (var item in seed)
            _items.Add(item);
    }

    public IReadOnlyList<T> GetAll()
    {
        lock (_lock) return _items.ToList();
    }

    public T? GetById(int id) =>
        _items.FirstOrDefault(item => item.Id == id);

    public IReadOnlyList<T> Find(Func<T, bool> predicate) =>
        _items.Where(predicate).ToList();

    public T Add(T entity)
    {
        if (entity.Id == 0)
        {
            entity.Id = _items.Count == 0 ? 1 : _items.Max(item => item.Id) + 1;
        }
        else if (GetById(entity.Id) is not null)
        {
            throw new InvalidOperationException(
                $"Сущность {typeof(T).Name} с ключом {entity.Id} уже существует");
        }
        lock (_lock) _items.Add(entity);
        return entity;
    }

    public bool Update(T entity)
    {
        var index = _items.FindIndex(item => item.Id == entity.Id);
        if (index < 0) return false;
        _items[index] = entity;
        return true;
    }

    public bool Delete(int id)
    {
        var entity = GetById(id);
        if (entity is null) return false;
        lock (_lock) _items.Remove(entity);
        return true;
    }

    /// <summary>Для хранилища в памяти сохранение не требуется.</summary>
    public void Save() { }
}

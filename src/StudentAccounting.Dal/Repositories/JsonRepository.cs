using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using StudentAccounting.Dal.Entities;

namespace StudentAccounting.Dal.Repositories;

/// <summary>
/// Репозиторий, сохраняющий сущности в JSON-файл (System.Text.Json).
/// Данные читаются при создании и записываются при вызове Save().
/// </summary>
public class JsonRepository<T> : IRepository<T> where T : class, IEntity
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };

    private readonly string _filePath;
    private List<T> _items;

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
        _items = Load();
    }

    private List<T> Load()
    {
        if (!File.Exists(_filePath)) return new List<T>();
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
    }

    public IReadOnlyList<T> GetAll() => _items.ToList();

    public T? GetById(int id) => _items.FirstOrDefault(item => item.Id == id);

    public IReadOnlyList<T> Find(Func<T, bool> predicate) => _items.Where(predicate).ToList();

    public T Add(T entity)
    {
        if (entity.Id == 0 || GetById(entity.Id) is not null)
            entity.Id = _items.Count == 0 ? 1 : _items.Max(item => item.Id) + 1;
        _items.Add(entity);
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
        _items.Remove(entity);
        return true;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_items, Options);
        File.WriteAllText(_filePath, json);
    }
}

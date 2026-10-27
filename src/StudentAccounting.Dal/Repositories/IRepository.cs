using StudentAccounting.Dal.Entities;

namespace StudentAccounting.Dal.Repositories;

/// <summary>
/// Универсальный контракт хранилища сущностей (паттерн «Репозиторий»).
/// Слой BLL работает только с этим интерфейсом и не знает, где именно
/// физически хранятся данные: в памяти, в JSON-файле или в СУБД.
/// </summary>
public interface IRepository<T> where T : class, IEntity
{
    /// <summary>Возвращает все сущности хранилища.</summary>
    IReadOnlyList<T> GetAll();

    /// <summary>Находит сущность по первичному ключу.</summary>
    T? GetById(int id);

    /// <summary>Выбирает сущности по условию.</summary>
    IReadOnlyList<T> Find(Func<T, bool> predicate);

    /// <summary>Добавляет новую сущность (ключ формируется автоматически).</summary>
    T Add(T entity);

    /// <summary>Обновляет существующую сущность по ключу.</summary>
    bool Update(T entity);

    /// <summary>Удаляет сущность по ключу; true, если она существовала.</summary>
    bool Delete(int id);

    /// <summary>Сохраняет изменения в физическое хранилище.</summary>
    void Save();
}

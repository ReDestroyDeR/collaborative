namespace StudentAccounting.Dal.Entities;

/// <summary>Маркерный интерфейс сущности с целочисленным ключом.</summary>
public interface IEntity
{
    /// <summary>Уникальный идентификатор сущности.</summary>
    int Id { get; set; }
}

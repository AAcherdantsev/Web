namespace Web.Domain.Entities;

/// <summary>
///     Base class for entities. The identifier is assigned by the store when the entity is added.
/// </summary>
public class BaseEntity
{
    /// <summary>
    ///     The unique identifier of the entity.
    /// </summary>
    public int Id { get; protected set; }
}

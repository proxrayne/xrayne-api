namespace Data.Entities;

public abstract class CreatedEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace Repositories.Entities;

public abstract class CreatedEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class CreateUpdateEntity : CreatedEntity
{
    public DateTimeOffset? UpdatedAt { get; set; }
}
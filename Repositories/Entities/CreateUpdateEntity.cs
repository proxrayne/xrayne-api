namespace Repositories.Entities;

public abstract class CreateUpdateEntity : CreatedEntity
{
    public DateTimeOffset? UpdatedAt { get; set; }
}

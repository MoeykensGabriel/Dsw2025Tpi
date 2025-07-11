using System.ComponentModel.DataAnnotations;

namespace Dsw2025Tpi.Domain.Entities;

public abstract class EntityBase
{
    [Key]
    public Guid Id { get; set; }
    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }
    protected EntityBase(Guid id)
    {
        this.Id = id;
    }

}

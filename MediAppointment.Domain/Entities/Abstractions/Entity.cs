
namespace MediAppointment.Domain.Entities.Abstractions
{
    public abstract class Entity<T> : IEntity<T>
    {
        public required T Id { get; set; }
    }
    public abstract class Entity  : Entity<Guid>;
    
}

using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Entities;

public abstract class Entity
{
    // Factories are evaluated lazily so that DB-generated IDs (assigned by EF
    // after SaveChangesAsync) are available when the event is actually built.
    private readonly List<Func<IDomainEvent>> _domainEventFactories = new();

    protected Entity()
    {
    }

    /// <summary>
    /// Registers a deferred event factory. The factory is executed after
    /// SaveChangesAsync so that any database-generated IDs on <c>this</c>
    /// entity are already populated.
    /// </summary>
    public void RaiseDomainEvent(Func<IDomainEvent> factory)
    {
        _domainEventFactories.Add(factory);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return _domainEventFactories.Select(f => f()).ToList();
    }

    public void ClearDomainEvents()
    {
        _domainEventFactories.Clear();
    }
}

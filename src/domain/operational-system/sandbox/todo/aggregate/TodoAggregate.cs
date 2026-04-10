using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class TodoAggregate : AggregateRoot
{
    public TodoId Id { get; private set; }
    private string _title = string.Empty;
    private bool _isCompleted;

    private TodoAggregate() { }

    public static TodoAggregate Create(TodoId id, string title)
    {
        Guard.Against(string.IsNullOrWhiteSpace(title), TodoErrors.TitleRequired);

        var aggregate = new TodoAggregate { Id = id };
        aggregate.RaiseDomainEvent(new TodoCreatedEvent(new AggregateId(id.Value), title));
        return aggregate;
    }

    public void Update(string title)
    {
        Guard.Against(string.IsNullOrWhiteSpace(title), TodoErrors.TitleRequired);
        Guard.Against(_isCompleted, TodoErrors.CannotUpdateCompleted);

        RaiseDomainEvent(new TodoUpdatedEvent(new AggregateId(Id.Value), title));
    }

    public void Complete()
    {
        Guard.Against(_isCompleted, TodoErrors.AlreadyCompleted);

        RaiseDomainEvent(new TodoCompletedEvent(new AggregateId(Id.Value)));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TodoCreatedEvent e:
                Id = new TodoId(e.AggregateId.Value);
                _title = e.Title;
                _isCompleted = false;
                break;

            case TodoUpdatedEvent e:
                _title = e.Title;
                break;

            case TodoCompletedEvent:
                _isCompleted = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }
}

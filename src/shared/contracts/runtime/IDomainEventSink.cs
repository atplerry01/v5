namespace Whyce.Shared.Contracts.Runtime;

/// <summary>
/// Surface used by execution paths (engines, workflow engine) to emit domain
/// events into the runtime pipeline. The runtime drains the sink after
/// successful execution and routes events through persist → chain → outbox.
///
/// This contract is the canonical replacement for the deprecated
/// IWorkflowStepObserver, which permitted runtime-side state mutation. With
/// IDomainEventSink, all lifecycle transitions become domain events.
/// </summary>
public interface IDomainEventSink
{
    void EmitEvent(object domainEvent);
    void EmitEvents(IEnumerable<object> domainEvents);
    IReadOnlyList<object> EmittedEvents { get; }
}

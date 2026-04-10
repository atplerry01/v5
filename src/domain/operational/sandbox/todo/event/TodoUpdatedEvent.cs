using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.Operational.Sandbox.Todo;

public sealed record TodoUpdatedEvent(AggregateId AggregateId, string Title) : DomainEvent;

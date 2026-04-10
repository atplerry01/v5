using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.Operational.Sandbox.Todo;

public sealed record TodoCreatedEvent(AggregateId AggregateId, string Title) : DomainEvent;

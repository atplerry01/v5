using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed record TodoCompletedEvent(AggregateId AggregateId) : DomainEvent;

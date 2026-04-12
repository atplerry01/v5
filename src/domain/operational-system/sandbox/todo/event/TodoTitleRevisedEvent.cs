using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed record TodoTitleRevisedEvent(AggregateId AggregateId, string Title) : DomainEvent;

using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed record ExecutionAbortedEvent(
    ExecutionId ExecutionId,
    string Reason,
    Timestamp AbortedAt) : DomainEvent;

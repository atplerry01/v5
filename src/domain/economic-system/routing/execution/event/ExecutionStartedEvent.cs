using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed record ExecutionStartedEvent(
    ExecutionId ExecutionId,
    PathId PathId,
    Timestamp StartedAt) : DomainEvent;

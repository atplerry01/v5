using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Reputation;

public sealed record ReputationUpdatedEvent(
    Guid ReputationId,
    double NewScore
) : DomainEvent;

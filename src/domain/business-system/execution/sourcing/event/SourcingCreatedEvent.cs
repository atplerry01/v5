namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed record SourcingCreatedEvent(SourcingId SourcingId, SourcingRequestId RequestId);

namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed record SourcingFailedEvent(SourcingId SourcingId, string Reason);

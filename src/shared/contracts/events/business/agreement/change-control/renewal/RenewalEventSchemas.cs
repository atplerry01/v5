namespace Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Renewal;

public sealed record RenewalCreatedEventSchema(Guid AggregateId, Guid SourceId);

public sealed record RenewalRenewedEventSchema(Guid AggregateId);

public sealed record RenewalExpiredEventSchema(Guid AggregateId);

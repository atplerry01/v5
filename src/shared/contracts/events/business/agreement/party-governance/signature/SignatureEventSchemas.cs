namespace Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Signature;

public sealed record SignatureCreatedEventSchema(Guid AggregateId);

public sealed record SignatureSignedEventSchema(Guid AggregateId);

public sealed record SignatureRevokedEventSchema(Guid AggregateId);

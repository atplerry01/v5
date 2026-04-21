namespace Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Account;

public sealed record AccountCreatedEventSchema(
    Guid AggregateId,
    Guid CustomerId,
    string Name,
    string Type);

public sealed record AccountRenamedEventSchema(Guid AggregateId, string Name);

public sealed record AccountActivatedEventSchema(Guid AggregateId);

public sealed record AccountSuspendedEventSchema(Guid AggregateId);

public sealed record AccountClosedEventSchema(Guid AggregateId);

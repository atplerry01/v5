namespace Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Customer;

public sealed record CustomerCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Type,
    string? ReferenceCode);

public sealed record CustomerRenamedEventSchema(Guid AggregateId, string Name);

public sealed record CustomerReclassifiedEventSchema(Guid AggregateId, string Type);

public sealed record CustomerActivatedEventSchema(Guid AggregateId);

public sealed record CustomerArchivedEventSchema(Guid AggregateId);

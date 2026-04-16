namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Contract;

public sealed record ContractShareRuleSchema(Guid PartyId, decimal SharePercentage);

public sealed record RevenueContractCreatedEventSchema(
    Guid AggregateId,
    IReadOnlyList<ContractShareRuleSchema> ShareRules,
    DateTimeOffset TermStart,
    DateTimeOffset TermEnd,
    DateTimeOffset CreatedAt);

public sealed record RevenueContractActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record RevenueContractTerminatedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset TerminatedAt);

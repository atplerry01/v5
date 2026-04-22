using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Schema.Contract;

public sealed record RegisterContractCommand(
    Guid ContractId,
    string ContractName,
    string PublisherClassification,
    string PublisherContext,
    string PublisherDomain,
    Guid SchemaRef,
    int SchemaVersion,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record AddContractSubscriberCommand(
    Guid ContractId,
    string SubscriberClassification,
    string SubscriberContext,
    string SubscriberDomain,
    int MinSchemaVersion,
    string RequiredCompatibilityMode,
    DateTimeOffset AddedAt) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record DeprecateContractCommand(
    Guid ContractId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

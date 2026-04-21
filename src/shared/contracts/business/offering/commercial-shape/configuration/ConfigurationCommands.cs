using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;

public sealed record CreateConfigurationCommand(
    Guid ConfigurationId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => ConfigurationId;
}

public sealed record SetConfigurationOptionCommand(
    Guid ConfigurationId,
    string Key,
    string Value) : IHasAggregateId
{
    public Guid AggregateId => ConfigurationId;
}

public sealed record RemoveConfigurationOptionCommand(
    Guid ConfigurationId,
    string Key) : IHasAggregateId
{
    public Guid AggregateId => ConfigurationId;
}

public sealed record ActivateConfigurationCommand(Guid ConfigurationId) : IHasAggregateId
{
    public Guid AggregateId => ConfigurationId;
}

public sealed record ArchiveConfigurationCommand(Guid ConfigurationId) : IHasAggregateId
{
    public Guid AggregateId => ConfigurationId;
}

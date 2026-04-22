using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;

public sealed record SetConfigurationStateCommand(
    Guid StateId,
    string DefinitionId,
    string Value,
    int Version) : IHasAggregateId
{
    public Guid AggregateId => StateId;
}

public sealed record RevokeConfigurationStateCommand(
    Guid StateId) : IHasAggregateId
{
    public Guid AggregateId => StateId;
}

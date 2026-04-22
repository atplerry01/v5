using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;

public sealed record RecordConfigurationResolutionCommand(
    Guid ResolutionId,
    string DefinitionId,
    string ScopeId,
    string StateId,
    string ResolvedValue,
    DateTimeOffset ResolvedAt) : IHasAggregateId
{
    public Guid AggregateId => ResolutionId;
}

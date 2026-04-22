using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Schema.Versioning;

public sealed record RegisterVersioningRuleCommand(
    Guid VersioningRuleId,
    Guid SchemaRef,
    int FromVersion,
    int ToVersion,
    string EvolutionClass,
    IReadOnlyList<SchemaChangeDto> ChangeSummary,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => VersioningRuleId;
}

public sealed record IssueVersioningVerdictCommand(
    Guid VersioningRuleId,
    string Verdict,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => VersioningRuleId;
}

public sealed record SchemaChangeDto(string ChangeType, string FieldName, string Impact);

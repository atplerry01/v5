using Whycespace.Shared.Contracts.Events.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;

namespace Whycespace.Projections.Platform.Schema.Versioning.Reducer;

public static class VersioningRuleProjectionReducer
{
    public static VersioningRuleReadModel Apply(VersioningRuleReadModel state, VersioningRuleRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            VersioningRuleId = e.AggregateId,
            SchemaRef = e.SchemaRef,
            FromVersion = e.FromVersion,
            ToVersion = e.ToVersion,
            EvolutionClass = e.EvolutionClass,
            LastModifiedAt = at
        };

    public static VersioningRuleReadModel Apply(VersioningRuleReadModel state, VersioningRuleVerdictIssuedEventSchema e, DateTimeOffset at) =>
        state with { Verdict = e.Verdict, LastModifiedAt = at };
}

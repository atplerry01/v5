using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDefinition;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyDefinition.Reducer;

public static class PolicyDefinitionProjectionReducer
{
    public static PolicyDefinitionReadModel Apply(PolicyDefinitionReadModel state, PolicyDefinedEventSchema e) =>
        state with
        {
            PolicyId            = e.AggregateId,
            Name                = e.Name,
            ScopeClassification = e.ScopeClassification,
            ScopeContext        = e.ScopeContext,
            ScopeActionMask     = e.ScopeActionMask,
            Version             = e.Version,
            Status              = "Active"
        };

    public static PolicyDefinitionReadModel Apply(PolicyDefinitionReadModel state, PolicyDeprecatedEventSchema e) =>
        state with { Status = "Deprecated" };
}

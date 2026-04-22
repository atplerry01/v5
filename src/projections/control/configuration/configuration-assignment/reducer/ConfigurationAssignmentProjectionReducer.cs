using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationAssignment;

namespace Whycespace.Projections.Control.Configuration.ConfigurationAssignment.Reducer;

public static class ConfigurationAssignmentProjectionReducer
{
    public static ConfigurationAssignmentReadModel Apply(ConfigurationAssignmentReadModel state, ConfigurationAssignedEventSchema e) =>
        state with
        {
            AssignmentId = e.AggregateId,
            DefinitionId = e.DefinitionId,
            ScopeId      = e.ScopeId,
            Value        = e.Value,
            Status       = "Active"
        };

    public static ConfigurationAssignmentReadModel Apply(ConfigurationAssignmentReadModel state, ConfigurationAssignmentRevokedEventSchema e) =>
        state with { Status = "Revoked" };
}

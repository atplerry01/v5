namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public static class PolicyDefinitionSpecification
{
    public static bool CanBeDeprecated(PolicyDefinitionAggregate policy) =>
        policy.Status == PolicyDefinitionStatus.Published;
}

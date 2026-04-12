namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public static class CanPublishSpecification
{
    public static bool IsSatisfiedBy(DefinitionStatus status) => status == DefinitionStatus.Draft;
}

public static class CanRetireSpecification
{
    public static bool IsSatisfiedBy(DefinitionStatus status) => status == DefinitionStatus.Published;
}

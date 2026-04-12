namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public static class DefinitionErrors
{
    public const string MissingId = "Definition identifier is required and must not be empty.";
    public const string MissingBlueprint = "A valid workflow blueprint is required.";

    public static InvalidOperationException InvalidStateTransition(DefinitionStatus status, string action) =>
        new($"Cannot {action} a definition in {status} status.");
}

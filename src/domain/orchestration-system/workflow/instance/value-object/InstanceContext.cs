namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public readonly record struct InstanceContext
{
    public Guid DefinitionReference { get; }
    public string InstanceName { get; }

    public InstanceContext(Guid definitionReference, string instanceName)
    {
        if (definitionReference == Guid.Empty)
            throw InstanceErrors.MissingContext("DefinitionReference cannot be empty.");

        if (string.IsNullOrWhiteSpace(instanceName))
            throw InstanceErrors.MissingContext("InstanceName cannot be empty.");

        DefinitionReference = definitionReference;
        InstanceName = instanceName;
    }
}

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public readonly record struct InstanceId
{
    public Guid Value { get; }

    public InstanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw InstanceErrors.MissingId();

        Value = value;
    }
}

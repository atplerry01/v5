namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaType
{
    public string Value { get; }

    public SagaType(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;
}
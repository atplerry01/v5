namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public readonly record struct CompletionTargetId
{
    public Guid Value { get; }

    public CompletionTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CompletionTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}

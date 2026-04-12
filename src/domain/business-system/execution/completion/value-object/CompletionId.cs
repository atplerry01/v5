namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public readonly record struct CompletionId
{
    public Guid Value { get; }

    public CompletionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CompletionId value must not be empty.", nameof(value));
        Value = value;
    }
}

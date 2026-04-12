namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public readonly record struct CallbackDefinitionId
{
    public Guid Value { get; }

    public CallbackDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CallbackDefinitionId value must not be empty.", nameof(value));
        Value = value;
    }
}

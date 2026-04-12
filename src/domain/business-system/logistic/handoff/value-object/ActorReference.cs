namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public readonly record struct ActorReference
{
    public Guid Value { get; }

    public ActorReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ActorReference value must not be empty.", nameof(value));
        Value = value;
    }
}

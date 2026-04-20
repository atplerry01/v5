namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public readonly record struct AmendmentTargetId
{
    public Guid Value { get; }

    public AmendmentTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AmendmentTargetId value must not be empty.", nameof(value));

        Value = value;
    }
}

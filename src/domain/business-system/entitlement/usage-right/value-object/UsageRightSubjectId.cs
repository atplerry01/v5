namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public readonly record struct UsageRightSubjectId
{
    public Guid Value { get; }

    public UsageRightSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UsageRightSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}

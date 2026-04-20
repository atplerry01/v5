namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public readonly record struct ApprovalId
{
    public Guid Value { get; }

    public ApprovalId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ApprovalId value must not be empty.", nameof(value));

        Value = value;
    }
}

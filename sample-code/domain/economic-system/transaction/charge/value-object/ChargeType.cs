namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeType
{
    public string Code { get; }
    public string Description { get; }

    public ChargeType(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("CHARGE.INVALID_TYPE_CODE", "Charge type code is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("CHARGE.INVALID_TYPE_DESCRIPTION", "Charge type description is required.");

        Code = code;
        Description = description;
    }
}

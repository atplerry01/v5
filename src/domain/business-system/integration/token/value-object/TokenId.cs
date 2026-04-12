namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public readonly record struct TokenId
{
    public Guid Value { get; }

    public TokenId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TokenId value must not be empty.", nameof(value));
        Value = value;
    }
}

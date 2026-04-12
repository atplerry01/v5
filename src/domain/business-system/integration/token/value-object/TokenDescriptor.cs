namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public readonly record struct TokenDescriptor
{
    public Guid PartnerReference { get; }
    public string TokenType { get; }

    public TokenDescriptor(Guid partnerReference, string tokenType)
    {
        if (partnerReference == Guid.Empty)
            throw new ArgumentException("PartnerReference must not be empty.", nameof(partnerReference));
        if (string.IsNullOrWhiteSpace(tokenType))
            throw new ArgumentException("TokenType must not be empty.", nameof(tokenType));

        PartnerReference = partnerReference;
        TokenType = tokenType;
    }
}

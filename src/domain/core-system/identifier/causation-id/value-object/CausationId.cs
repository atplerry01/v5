namespace Whycespace.Domain.CoreSystem.Identifier.CausationId;

/// <summary>
/// Causal link recording which command or event directly caused the current message.
/// Where CorrelationId groups a logical operation, CausationId expresses the direct parent-child
/// relationship between messages. SHA256-derived (64 lowercase hex chars).
/// Never generated inside the domain — assigned by engine from the inbound message ID.
/// </summary>
public readonly record struct CausationId
{
    private const int RequiredLength = 64;

    public string Value { get; }

    public CausationId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw CausationIdErrors.ValueMustNotBeEmpty();

        if (value.Length != RequiredLength || !IsLowercaseHex(value))
            throw CausationIdErrors.ValueMustBe64LowercaseHexChars(value);

        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                return false;
        return true;
    }

    public override string ToString() => Value;
}

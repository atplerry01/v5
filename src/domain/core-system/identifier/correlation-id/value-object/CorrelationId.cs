namespace Whycespace.Domain.CoreSystem.Identifier.CorrelationId;

/// <summary>
/// Cross-boundary correlation key linking all messages within a single logical operation.
/// All commands and events produced in response to the same root trigger carry the same CorrelationId.
/// SHA256-derived (64 lowercase hex chars). Never generated inside the domain — assigned by engine.
/// </summary>
public readonly record struct CorrelationId
{
    private const int RequiredLength = 64;

    public string Value { get; }

    public CorrelationId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw CorrelationIdErrors.ValueMustNotBeEmpty();

        if (value.Length != RequiredLength || !IsLowercaseHex(value))
            throw CorrelationIdErrors.ValueMustBe64LowercaseHexChars(value);

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

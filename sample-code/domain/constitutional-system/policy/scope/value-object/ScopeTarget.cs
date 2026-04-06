namespace Whycespace.Domain.ConstitutionalSystem.Policy.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class ScopeTarget : ValueObject
{
    public string Identifier { get; }
    public bool IsWildcard { get; }

    private ScopeTarget(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        Identifier = identifier;
        IsWildcard = identifier == "*";
    }

    public static ScopeTarget For(string identifier) => new(identifier);

    public static ScopeTarget All() => new("*");

    public bool Matches(string targetIdentifier)
    {
        if (IsWildcard) return true;
        return string.Equals(Identifier, targetIdentifier, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Identifier;
    }

    public override string ToString() => Identifier;
}

namespace Whycespace.Domain.CoreSystem.Identifier.EntityReference;

// Typed reference: EntityType follows {classification}/{context}/{domain} format.
public sealed record EntityReference
{
    public string IdentifierValue { get; }
    public string EntityType { get; }

    public EntityReference(string identifierValue, string entityType)
    {
        if (string.IsNullOrEmpty(identifierValue))
            throw EntityReferenceErrors.IdentifierMustNotBeEmpty();

        if (identifierValue.Length != 64 || !IsLowercaseHex(identifierValue))
            throw EntityReferenceErrors.IdentifierMustBe64LowercaseHexChars(identifierValue);

        if (string.IsNullOrEmpty(entityType))
            throw EntityReferenceErrors.EntityTypeMustNotBeEmpty();

        var segments = entityType.Split('/');
        if (segments.Length != 3 || segments[0].Length == 0 || segments[1].Length == 0 || segments[2].Length == 0)
            throw EntityReferenceErrors.EntityTypeMustFollowThreeSegmentFormat(entityType);

        IdentifierValue = identifierValue;
        EntityType = entityType;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                return false;
        return true;
    }

    // Two references point to the same entity when both type and identifier match.
    public bool IsSameEntity(EntityReference other) =>
        string.Equals(EntityType, other.EntityType, StringComparison.Ordinal) &&
        string.Equals(IdentifierValue, other.IdentifierValue, StringComparison.Ordinal);

    public override string ToString() => $"{EntityType}:{IdentifierValue}";
}

namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public static class SchemaErrors
{
    public static SchemaDomainException MissingId()
        => new("SchemaId is required and must not be empty.");

    public static SchemaDomainException MissingDefinitionId()
        => new("SchemaDefinitionId is required and must not be empty.");

    public static SchemaDomainException AlreadyPublished(SchemaId id)
        => new($"Schema '{id.Value}' has already been published.");

    public static SchemaDomainException AlreadyFinalized(SchemaId id)
        => new($"Schema '{id.Value}' has already been finalized and is immutable.");

    public static SchemaDomainException InvalidStateTransition(SchemaStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SchemaDomainException : Exception
{
    public SchemaDomainException(string message) : base(message) { }
}

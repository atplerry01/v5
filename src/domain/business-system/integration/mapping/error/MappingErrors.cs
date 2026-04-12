namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public static class MappingErrors
{
    public static MappingDomainException MissingId()
        => new("MappingId is required and must not be empty.");

    public static MappingDomainException MissingDefinitionId()
        => new("MappingDefinitionId is required and must not be empty.");

    public static MappingDomainException InvalidStateTransition(MappingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static MappingDomainException AlreadyActive(MappingId id)
        => new($"Mapping '{id.Value}' is already active.");

    public static MappingDomainException AlreadyDisabled(MappingId id)
        => new($"Mapping '{id.Value}' is already disabled.");
}

public sealed class MappingDomainException : Exception
{
    public MappingDomainException(string message) : base(message) { }
}

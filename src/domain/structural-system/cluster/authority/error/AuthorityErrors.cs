namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public static class AuthorityErrors
{
    public static InvalidOperationException MissingId()
        => new("Authority ID must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("Authority descriptor must have a non-empty cluster reference and authority name.");

    public static InvalidOperationException InvalidStateTransition(AuthorityStatus status, string action)
        => new($"Cannot perform '{action}' when authority status is '{status}'.");
}

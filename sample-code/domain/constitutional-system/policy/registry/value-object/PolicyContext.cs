namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyContext : ValueObject
{
    public Guid? PolicyId { get; }
    public Guid ActorId { get; }
    public string Action { get; }
    public string Resource { get; }
    public string Environment { get; }
    public DateTimeOffset Timestamp { get; }

    private PolicyContext(
        Guid? policyId,
        Guid actorId,
        string action,
        string resource,
        string environment,
        DateTimeOffset timestamp)
    {
        PolicyId = policyId;
        ActorId = actorId;
        Action = action;
        Resource = resource;
        Environment = environment;
        Timestamp = timestamp;
    }

    public static PolicyContext Create(
        Guid actorId,
        string action,
        string resource,
        string environment,
        DateTimeOffset timestamp,
        Guid? policyId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        return new PolicyContext(
            policyId,
            actorId,
            action,
            resource,
            environment,
            timestamp);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PolicyId ?? Guid.Empty;
        yield return ActorId;
        yield return Action;
        yield return Resource;
        yield return Environment;
        yield return Timestamp;
    }

    public override string ToString() =>
        $"Actor:{ActorId} Action:{Action} Resource:{Resource} Env:{Environment}";
}

namespace Whyce.Shared.Contracts.Runtime;

public interface ISystemIntentDispatcher
{
    Task<CommandResult> DispatchAsync(object command, DomainRoute route);
}

/// <summary>
/// Domain routing metadata for canonical Kafka topic resolution.
/// Populated by the entry point (intent handler, controller) and threaded through
/// the dispatcher chain into CommandContext → EventEnvelope → TopicNameResolver.
/// </summary>
public sealed record DomainRoute(string Classification, string Context, string Domain);

namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B — minimal transport abstraction for DLQ re-drive. The admin service
/// owns eligibility + audit; the publisher owns the actual re-publication
/// onto the source topic. Keeping this separate lets tests exercise the
/// eligibility + audit seams against an in-memory publisher and lets the
/// Kafka adapter evolve independently.
///
/// <para>Publishers MUST preserve the original <paramref name="payload"/>
/// bytes verbatim. Translation / reserialisation is out of scope for R4.B —
/// if schema evolution is required, that's a separate migration concern.</para>
/// </summary>
public interface IDeadLetterRedrivePublisher
{
    /// <summary>
    /// Publish <paramref name="payload"/> back onto <paramref name="topic"/>.
    /// Throws a <see cref="DeadLetterRedrivePublishException"/> on transport
    /// failure so the service can surface the reason in a
    /// <see cref="DeadLetterRedriveResult"/>.
    /// </summary>
    Task PublishAsync(
        string topic,
        byte[] payload,
        Guid eventId,
        string eventType,
        int? schemaVersion,
        Guid correlationId,
        CancellationToken cancellationToken = default);
}

public sealed class DeadLetterRedrivePublishException : Exception
{
    public DeadLetterRedrivePublishException(string message) : base(message) { }
    public DeadLetterRedrivePublishException(string message, Exception inner) : base(message, inner) { }
}

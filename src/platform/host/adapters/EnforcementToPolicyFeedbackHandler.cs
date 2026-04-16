using Confluent.Kafka;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Constitutional.Policy.Feedback;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Enforcement → Policy feedback bridge. Consumes violation-detected and
/// violation-resolved envelopes and publishes a
/// <see cref="PolicyFeedbackEventSchema"/> to
/// <c>whyce.constitutional.policy.feedback.events</c> so WhycePolicy can
/// factor active enforcement state into subsequent decisions.
///
/// Lives in platform/host/adapters because it depends on the Kafka producer;
/// runtime layer is prohibited from referencing Confluent.Kafka directly.
/// Event-id is derived deterministically via <see cref="IIdGenerator"/> so
/// replayed source envelopes produce byte-identical feedback emissions.
/// </summary>
public sealed class EnforcementToPolicyFeedbackHandler
{
    public const string FeedbackTopic = "whyce.constitutional.policy.feedback.events";
    private const string OutcomeDetected = "ViolationDetected";
    private const string OutcomeResolved = "ViolationResolved";
    private const string EventTypeName = "PolicyFeedbackEvent";

    private readonly IProducer<string, string> _producer;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;
    private readonly string _policyVersion;

    public EnforcementToPolicyFeedbackHandler(
        IProducer<string, string> producer,
        IClock clock,
        IIdGenerator idGenerator,
        string policyVersion)
    {
        _producer = producer;
        _clock = clock;
        _idGenerator = idGenerator;
        _policyVersion = policyVersion;
    }

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var feedback = envelope.Payload switch
        {
            ViolationDetectedEventSchema d => new PolicyFeedbackEventSchema(
                SubjectId: d.SourceReference,
                ViolationId: d.AggregateId,
                Severity: d.Severity,
                Action: d.RecommendedAction,
                Outcome: OutcomeDetected,
                PolicyVersion: _policyVersion,
                Timestamp: _clock.UtcNow),
            ViolationResolvedEventSchema r => new PolicyFeedbackEventSchema(
                SubjectId: Guid.Empty,
                ViolationId: r.AggregateId,
                Severity: string.Empty,
                Action: "Cleared",
                Outcome: OutcomeResolved,
                PolicyVersion: _policyVersion,
                Timestamp: _clock.UtcNow),
            _ => null
        };

        if (feedback is null) return Task.CompletedTask;

        var derivedEventId = _idGenerator.Generate(
            $"policy-feedback:{envelope.EventId}:{feedback.Outcome}");

        var payload = EventSerializer.Serialize(feedback);
        var headers = new Headers
        {
            { "event-id",       System.Text.Encoding.UTF8.GetBytes(derivedEventId.ToString()) },
            { "aggregate-id",   System.Text.Encoding.UTF8.GetBytes(feedback.ViolationId.ToString()) },
            { "event-type",     System.Text.Encoding.UTF8.GetBytes(EventTypeName) },
            { "correlation-id", System.Text.Encoding.UTF8.GetBytes(envelope.CorrelationId.ToString()) }
        };

        return _producer.ProduceAsync(
            FeedbackTopic,
            new Message<string, string>
            {
                Key = feedback.ViolationId.ToString(),
                Value = payload,
                Headers = headers
            },
            cancellationToken);
    }
}

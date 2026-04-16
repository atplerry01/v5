namespace Whycespace.Shared.Contracts.Events.Constitutional.Policy.Feedback;

/// <summary>
/// Cross-domain feedback signal published by the enforcement subsystem when
/// a violation is detected or resolved. Target topic:
///   whyce.constitutional.policy.feedback.events
///
/// Consumers: WhycePolicy evaluator + policy observability. The feedback
/// loop allows active enforcement state to influence future policy
/// decisions (e.g. raising restriction for a subject with active
/// Critical violations).
/// </summary>
public sealed record PolicyFeedbackEventSchema(
    Guid SubjectId,
    Guid ViolationId,
    string Severity,
    string Action,
    string Outcome,
    string PolicyVersion,
    DateTimeOffset Timestamp);

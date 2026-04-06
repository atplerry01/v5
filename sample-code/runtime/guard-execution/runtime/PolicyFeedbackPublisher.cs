using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.GuardExecution.Engine;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Publishes guard violations back into:
/// - WHYCEPOLICY simulation engine (for policy tuning)
/// - Governance Assist (T3I) for compliance tracking
/// </summary>
public sealed class PolicyFeedbackPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IClock _clock;
    private readonly ILogger<PolicyFeedbackPublisher> _logger;

    public PolicyFeedbackPublisher(
        IEventPublisher eventPublisher,
        IClock clock,
        ILogger<PolicyFeedbackPublisher> logger)
    {
        _eventPublisher = eventPublisher;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Publishes violation feedback for policy simulation and governance tracking.
    /// </summary>
    public async Task PublishFeedbackAsync(
        GuardExecutionReport report,
        string correlationId,
        string commandName,
        CancellationToken cancellationToken = default)
    {
        if (report.AllViolations.Count == 0) return;

        // Feedback event for WHYCEPOLICY simulation engine
        var simulationFeedback = new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"guard:feedback:simulation:{correlationId}"),
            AggregateId = Guid.Empty,
            EventType = "GuardViolationFeedbackEvent",
            AggregateType = "PolicySimulation",
            CorrelationId = correlationId,
            Payload = new
            {
                commandName,
                violationCount = report.AllViolations.Count,
                violations = report.AllViolations.Select(v => new
                {
                    rule = v.Rule,
                    severity = v.Severity.ToString(),
                    file = v.File,
                    description = v.Description
                }),
                guardStatus = report.Status.ToString(),
                timestamp = _clock.UtcNowOffset
            },
            Timestamp = _clock.UtcNowOffset
        };

        // Governance compliance event for T3I Governance Assist
        var governanceFeedback = new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"guard:feedback:governance:{correlationId}"),
            AggregateId = Guid.Empty,
            EventType = "GovernanceComplianceViolationEvent",
            AggregateType = "GovernanceAssist",
            CorrelationId = correlationId,
            Payload = new
            {
                commandName,
                blockingViolations = report.BlockingViolations.Select(v => v.Rule),
                warningViolations = report.Warnings.Select(v => v.Rule),
                status = report.Status.ToString()
            },
            Timestamp = _clock.UtcNowOffset
        };

        await _eventPublisher.PublishAsync([simulationFeedback, governanceFeedback], cancellationToken);

        _logger.LogInformation(
            "Published guard feedback for {Command}: {ViolationCount} violations → PolicySimulation + GovernanceAssist",
            commandName, report.AllViolations.Count);
    }
}

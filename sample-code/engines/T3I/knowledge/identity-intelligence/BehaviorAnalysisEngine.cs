using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Identity;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// T3I Behavior Analysis Engine — analyzes session and usage patterns.
///
/// Generates behavior signals from raw event data.
/// Computes a deterministic pattern signature for comparison.
///
/// Stateless. No persistence.
/// Uses IIdentityIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class BehaviorAnalysisEngine
{
    private readonly IClock _clock;
    private readonly IIdentityIntelligenceDomainService _domainService;

    public BehaviorAnalysisEngine(IClock clock, IIdentityIntelligenceDomainService domainService)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<BehaviorResult> AnalyzeAsync(AnalyzeBehaviorCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var signals = new List<BehaviorSignalDto>();
        var now = _clock.UtcNowOffset;

        // Analyze session patterns
        foreach (var session in command.SessionEvents)
        {
            if (session.EndedAt.HasValue)
            {
                var duration = session.EndedAt.Value - session.StartedAt;
                if (duration.TotalHours > 12)
                {
                    signals.Add(new BehaviorSignalDto("long_session", 5m, session.StartedAt));
                }
            }
        }

        // Analyze event type frequency
        var typeCounts = command.RecentEventTypes
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .ToList();

        foreach (var group in typeCounts)
        {
            if (group.Count() > 10)
            {
                signals.Add(new BehaviorSignalDto(
                    $"high_frequency_{group.Key}",
                    group.Count() * 0.5m,
                    now));
            }
        }

        // Create behavior profile and record signals via domain service
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{command.IdentityId}:AnalyzeBehavior").ToString("N"),
            ActorId = "system",
            Action = "AnalyzeBehavior",
            Domain = "intelligence.identity",
            Timestamp = _clock.UtcNowOffset
        };

        var identityId = Guid.Parse(command.IdentityId);
        await _domainService.CreateBehaviorProfileAsync(execCtx, identityId);
        foreach (var signal in signals)
        {
            await _domainService.RecordBehaviorSignalAsync(execCtx, identityId, signal);
        }

        // Compute pattern signature deterministically
        var signatureInput = string.Join("|", signals.Select(s => $"{s.SignalType}:{s.Weight}"));
        var signatureBytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(signatureInput));
        var patternHash = Convert.ToHexStringLower(signatureBytes);

        return new BehaviorResult(
            command.IdentityId,
            signals,
            patternHash,
            signals.Count,
            now);
    }
}

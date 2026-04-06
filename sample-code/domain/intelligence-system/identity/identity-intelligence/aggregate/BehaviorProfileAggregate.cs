using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Intelligence behavior profile — tracks behavioral patterns for an identity.
/// Generates behavior signals from session and usage patterns.
/// </summary>
public sealed class BehaviorProfileAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public IReadOnlyList<BehaviorSignal> Signals => _signals.AsReadOnly();
    public PatternSignature? CurrentPattern { get; private set; }

    private readonly List<BehaviorSignal> _signals = [];

    private BehaviorProfileAggregate() { }

    public static BehaviorProfileAggregate Create(Guid identityId)
    {
        Guard.AgainstDefault(identityId);
        var profile = new BehaviorProfileAggregate { IdentityId = identityId };
        profile.Id = identityId;
        return profile;
    }

    public void RecordSignal(BehaviorSignal signal)
    {
        Guard.AgainstNull(signal);
        _signals.Add(signal);
    }

    /// <summary>
    /// Analyze recorded signals and compute a pattern signature.
    /// Deterministic: same signals produce the same signature.
    /// </summary>
    public PatternSignature Analyze()
    {
        if (_signals.Count == 0)
        {
            CurrentPattern = new PatternSignature("empty", 0, 0m, "none");
            return CurrentPattern;
        }

        var ordered = _signals.OrderBy(s => s.ObservedAt).ToList();
        var intervals = new List<decimal>();

        for (var i = 1; i < ordered.Count; i++)
        {
            var diff = (decimal)(ordered[i].ObservedAt - ordered[i - 1].ObservedAt).TotalSeconds;
            intervals.Add(diff);
        }

        var avgInterval = intervals.Count > 0 ? intervals.Average() : 0m;
        var dominant = _signals
            .GroupBy(s => s.SignalType)
            .OrderByDescending(g => g.Count())
            .First().Key;

        var signatureInput = $"{_signals.Count}|{avgInterval:F2}|{dominant}|{string.Join(",", _signals.Select(s => s.SignalType))}";
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(signatureInput)));

        CurrentPattern = new PatternSignature(hash, _signals.Count, avgInterval, dominant);

        RaiseDomainEvent(new BehaviorAnalyzedEvent(
            IdentityId,
            _signals.Select(s => s.SignalType).Distinct().ToList(),
            hash));

        return CurrentPattern;
    }
}

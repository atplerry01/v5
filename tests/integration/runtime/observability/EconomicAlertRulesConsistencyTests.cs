using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Runtime.Observability;

namespace Whycespace.Tests.Integration.Runtime.Observability;

/// <summary>
/// Phase 5 — Observability & Alert Integrity.
///
/// Pins the contract that every entry in
/// <see cref="EconomicAlertRules.All"/> points at a metric that is actually
/// published by one of the two canonical meters:
///   * <c>Whycespace.Economic.Business</c> — domain signals emitted from
///     T1M workflow steps through <c>IEconomicMetrics</c> /
///     <see cref="EconomicBusinessMetrics"/>.
///   * <c>Whycespace.Runtime.Enforcement</c> — enforcement middleware
///     signals emitted from <see cref="ExecutionGuardMiddleware"/>.
///
/// Phantom alerts (alert names pointing at metrics that do not exist on
/// either meter) are the exact failure mode this test is here to catch.
/// </summary>
public sealed class EconomicAlertRulesConsistencyTests
{
    private static readonly string[] CanonicalMeters =
    {
        "Whycespace.Economic.Business",
        "Whycespace.Runtime.Enforcement",
    };

    private static HashSet<string> CollectPublishedInstrumentNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (CanonicalMeters.Contains(instrument.Meter.Name))
                names.Add(instrument.Name);
        };
        listener.Start();

        // Listener is now active; force class initialisation so the
        // Meter.CreateCounter<T> calls in the static field initialisers
        // fire InstrumentPublished on the listener above. Using
        // RunClassConstructor keeps this deterministic even if another
        // test already loaded the type (it is idempotent per type).
        RuntimeHelpers.RunClassConstructor(typeof(EconomicBusinessMetrics).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ExecutionGuardMiddleware).TypeHandle);

        // Belt-and-braces: touch a member of each type so the runtime
        // observes a real access even when the cctor already ran
        // (published instruments are cached on the meter).
        _ = new EconomicBusinessMetrics();
        _ = new ExecutionGuardMiddleware();

        return names;
    }

    [Fact]
    public void EveryAlertRule_MapsTo_AnEmittedMetric()
    {
        var emitted = CollectPublishedInstrumentNames();

        var orphans = EconomicAlertRules.All
            .Where(rule => !emitted.Contains(rule.MetricName))
            .Select(rule => $"{rule.Name} -> {rule.MetricName}")
            .ToList();

        Assert.Empty(orphans);
    }

    [Fact]
    public void AlertRules_AreWellFormed()
    {
        foreach (var rule in EconomicAlertRules.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.Name), $"Alert name must be set ({rule.MetricName}).");
            Assert.False(string.IsNullOrWhiteSpace(rule.MetricName), $"MetricName must be set ({rule.Name}).");
            Assert.StartsWith("whyce.", rule.MetricName);
            Assert.True(rule.Threshold > 0, $"{rule.Name} threshold must be positive.");
            Assert.True(rule.WindowSeconds > 0, $"{rule.Name} window must be positive.");
            Assert.False(string.IsNullOrWhiteSpace(rule.Summary), $"{rule.Name} summary must be set.");
            Assert.False(string.IsNullOrWhiteSpace(rule.Runbook), $"{rule.Name} runbook must be set.");
        }
    }

    [Fact]
    public void AlertRules_HaveUniqueNames()
    {
        var duplicates = EconomicAlertRules.All
            .GroupBy(r => r.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Empty(duplicates);
    }
}

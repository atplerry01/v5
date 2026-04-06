using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.PolicySimulation.Scenario;

/// <summary>
/// Captures a consistent, immutable dataset snapshot for simulation.
/// Stateless — reads policy versions from IPolicyReadModel (projections).
/// All simulation runs against the same snapshot produce identical results.
/// </summary>
public sealed class SimulationSnapshotProvider
{
    private readonly IPolicyReadModel _readModel;

    public SimulationSnapshotProvider(IPolicyReadModel readModel)
    {
        _readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
    }

    public async Task<SimulationSnapshot> CaptureAsync(
        IReadOnlyList<PolicySimulationTarget> targets,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targets);

        var snapshotEntries = new List<SnapshotPolicyEntry>();

        foreach (var target in targets)
        {
            var version = target.Version.HasValue
                ? await _readModel.GetVersionAsync(target.PolicyId, target.Version.Value, cancellationToken)
                : await _readModel.GetActiveVersionAsync(target.PolicyId, cancellationToken);

            snapshotEntries.Add(new SnapshotPolicyEntry(
                target.PolicyId,
                version,
                target.IsOverride));
        }

        var snapshotId = ComputeSnapshotId(snapshotEntries, referenceTime);

        return new SimulationSnapshot(
            snapshotId,
            referenceTime,
            snapshotEntries.AsReadOnly());
    }

    public async Task<SimulationSnapshot> RestoreAsync(
        Guid snapshotId,
        IReadOnlyList<PolicySimulationTarget> targets,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await CaptureAsync(targets, referenceTime, cancellationToken);

        if (snapshot.SnapshotId != snapshotId)
            throw new InvalidOperationException(
                $"Snapshot mismatch: expected {snapshotId}, computed {snapshot.SnapshotId}. " +
                "Underlying data has changed since the original snapshot was taken.");

        return snapshot;
    }

    private static Guid ComputeSnapshotId(
        IReadOnlyList<SnapshotPolicyEntry> entries, DateTimeOffset referenceTime)
    {
        var sb = new StringBuilder();
        sb.Append(referenceTime.ToUnixTimeMilliseconds());

        foreach (var entry in entries.OrderBy(e => e.PolicyId))
        {
            sb.Append('|');
            sb.Append(entry.PolicyId);
            sb.Append(':');
            sb.Append(entry.Version?.Version ?? -1);
            sb.Append(':');
            sb.Append(entry.Version?.ArtifactHash ?? "none");
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return new Guid(hash.AsSpan(0, 16));
    }
}

public sealed record SimulationSnapshot(
    Guid SnapshotId,
    DateTimeOffset ReferenceTime,
    IReadOnlyList<SnapshotPolicyEntry> Entries);

public sealed record SnapshotPolicyEntry(
    Guid PolicyId,
    PolicyVersionRecord? Version,
    bool IsOverride);

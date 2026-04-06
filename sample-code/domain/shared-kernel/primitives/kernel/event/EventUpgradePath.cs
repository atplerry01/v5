namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Maps version-to-version transformations for event schema upgrades.
/// Upgrade happens during replay — persisted events are NEVER mutated.
/// </summary>
public sealed class EventUpgradePath
{
    private readonly Dictionary<string, SortedList<int, IEventUpgradeStep>> _paths = new();

    /// <summary>
    /// Registers a single upgrade step from one version to the next.
    /// Steps must be registered in order (v1→v2, v2→v3, etc.).
    /// </summary>
    public void RegisterStep(string schemaId, int fromVersion, IEventUpgradeStep step)
    {
        if (!_paths.TryGetValue(schemaId, out var steps))
        {
            steps = new SortedList<int, IEventUpgradeStep>();
            _paths[schemaId] = steps;
        }

        if (steps.ContainsKey(fromVersion))
            throw new DomainException(
                "DUPLICATE_UPGRADE_STEP",
                $"Upgrade step for '{schemaId}' from v{fromVersion} is already registered.");

        steps.Add(fromVersion, step);
    }

    /// <summary>
    /// Upgrades a raw event payload from its persisted version to the target version
    /// by chaining all intermediate upgrade steps. Returns the upgraded payload.
    /// The original payload is never mutated.
    /// </summary>
    public EventUpgradeResult Upgrade(string schemaId, int fromVersion, int toVersion, IReadOnlyDictionary<string, object?> payload)
    {
        if (fromVersion == toVersion)
            return new EventUpgradeResult { Payload = payload, ResultVersion = fromVersion, StepsApplied = 0 };

        if (fromVersion > toVersion)
            throw new DomainException(
                "INVALID_UPGRADE_DIRECTION",
                $"Cannot downgrade '{schemaId}' from v{fromVersion} to v{toVersion}. Event upgrades are forward-only.");

        if (!_paths.TryGetValue(schemaId, out var steps))
            throw new DomainException(
                "NO_UPGRADE_PATH",
                $"No upgrade path registered for '{schemaId}'.");

        var current = new Dictionary<string, object?>(payload);
        int stepsApplied = 0;

        for (int v = fromVersion; v < toVersion; v++)
        {
            if (!steps.TryGetValue(v, out var step))
                throw new DomainException(
                    "MISSING_UPGRADE_STEP",
                    $"No upgrade step registered for '{schemaId}' from v{v} to v{v + 1}.");

            current = new Dictionary<string, object?>(step.Upgrade(current));
            stepsApplied++;
        }

        return new EventUpgradeResult
        {
            Payload = current,
            ResultVersion = toVersion,
            StepsApplied = stepsApplied
        };
    }

    /// <summary>Returns true if a complete upgrade path exists between the two versions.</summary>
    public bool CanUpgrade(string schemaId, int fromVersion, int toVersion)
    {
        if (fromVersion >= toVersion) return fromVersion == toVersion;
        if (!_paths.TryGetValue(schemaId, out var steps)) return false;

        for (int v = fromVersion; v < toVersion; v++)
        {
            if (!steps.ContainsKey(v)) return false;
        }

        return true;
    }
}

/// <summary>
/// A single version-to-version upgrade step. Implementations transform the raw payload
/// from version N to version N+1. Must be pure — no side effects, no mutations.
/// </summary>
public interface IEventUpgradeStep
{
    /// <summary>
    /// Transforms the payload from version N to N+1.
    /// Must return a new dictionary — never mutate the input.
    /// </summary>
    IReadOnlyDictionary<string, object?> Upgrade(IReadOnlyDictionary<string, object?> payload);
}

/// <summary>
/// Result of applying an upgrade path to an event payload.
/// </summary>
public sealed record EventUpgradeResult
{
    public required IReadOnlyDictionary<string, object?> Payload { get; init; }
    public required int ResultVersion { get; init; }
    public required int StepsApplied { get; init; }
}

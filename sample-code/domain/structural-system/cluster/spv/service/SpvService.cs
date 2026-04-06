namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

/// <summary>
/// SPV lifecycle domain service — validates lifecycle transitions
/// and enforces business rules for SPV state changes.
/// </summary>
public sealed class SpvService
{
    /// <summary>
    /// Validates that an SPV can be terminated — must have no pending transactions.
    /// </summary>
    public SpvLifecycleResult ValidateTermination(
        SpvAggregate spv,
        bool hasPendingTransactions)
    {
        if (spv.Status.IsTerminal)
            return SpvLifecycleResult.Fail("SPV is already in a terminal state.");

        if (hasPendingTransactions)
            return SpvLifecycleResult.Fail("Cannot terminate SPV with pending cross-SPV transactions.");

        if (spv.Status != SpvStatus.Active && spv.Status != SpvStatus.Suspended)
            return SpvLifecycleResult.Fail($"Cannot terminate SPV in '{spv.Status.Value}' state.");

        return SpvLifecycleResult.Success();
    }

    /// <summary>
    /// Validates that an SPV can be closed — must be terminated and have an audit record.
    /// </summary>
    public SpvLifecycleResult ValidateClosure(
        SpvAggregate spv,
        Guid auditRecordId)
    {
        if (spv.Status != SpvStatus.Terminated)
            return SpvLifecycleResult.Fail("SPV must be terminated before closure.");

        if (auditRecordId == Guid.Empty)
            return SpvLifecycleResult.Fail("Audit record is required for SPV closure.");

        return SpvLifecycleResult.Success();
    }
}

public sealed record SpvLifecycleResult(bool IsValid, string? Error)
{
    public static SpvLifecycleResult Fail(string error) => new(false, error);
    public static SpvLifecycleResult Success() => new(true, null);
}

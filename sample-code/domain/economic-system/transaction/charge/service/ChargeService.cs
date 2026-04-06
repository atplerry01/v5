namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

/// <summary>
/// Charge domain service — stateless validation and business rule enforcement
/// for charge lifecycle operations.
/// </summary>
public sealed class ChargeService
{
    /// <summary>
    /// Validates that a charge can be reversed.
    /// </summary>
    public ChargeOperationResult ValidateReversal(ChargeAggregate charge)
    {
        if (charge.Status != ChargeStatus.Applied)
            return ChargeOperationResult.Fail($"Charge must be in 'applied' status to reverse. Current: '{charge.Status.Value}'.");

        return ChargeOperationResult.Success();
    }

    /// <summary>
    /// Validates that a charge can be waived.
    /// </summary>
    public ChargeOperationResult ValidateWaiver(ChargeAggregate charge)
    {
        if (charge.Status.IsTerminal)
            return ChargeOperationResult.Fail($"Charge in terminal status '{charge.Status.Value}' cannot be waived.");

        if (charge.Status == ChargeStatus.Applied)
            return ChargeOperationResult.Fail("Applied charges must be reversed, not waived.");

        return ChargeOperationResult.Success();
    }
}

public sealed record ChargeOperationResult(bool IsValid, string? Error)
{
    public static ChargeOperationResult Fail(string error) => new(false, error);
    public static ChargeOperationResult Success() => new(true, null);
}

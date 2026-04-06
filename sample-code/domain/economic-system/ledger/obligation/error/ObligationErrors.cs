namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public static class ObligationErrors
{
    public static DomainException AlreadySettled(Guid obligationId) =>
        new("OBLIGATION_ALREADY_SETTLED", $"Obligation {obligationId} is already settled.");

    public static DomainException InvalidState(Guid obligationId, string currentState, string attemptedAction) =>
        new("OBLIGATION_INVALID_STATE", $"Cannot {attemptedAction} obligation {obligationId} in '{currentState}' state.");

    public static DomainException InvalidAmount() =>
        new("OBLIGATION_INVALID_AMOUNT", "Obligation amount must be positive.");
}

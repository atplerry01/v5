namespace Whycespace.Domain.IntelligenceSystem.Economic.Integrity;

public sealed class IntelligenceIntegrityAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid? WalletId { get; private set; }
    public decimal IntegrityScore { get; private set; }
    public decimal CalibratedConfidence { get; private set; }
    public bool ConflictDetected { get; private set; }
    public string? ConflictReason { get; private set; }
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    // Scope
    public AnalysisScope Scope { get; private set; } = AnalysisScope.Identity;

    private IntelligenceIntegrityAggregate() { }

    public static IntelligenceIntegrityAggregate Create(
        Guid integrityId,
        Guid identityId,
        Guid? walletId,
        AnalysisScope scope,
        decimal integrityScore,
        decimal calibratedConfidence,
        bool conflictDetected,
        string? conflictReason,
        string correlationId,
        ObservationWindow window)
    {
        Guard.AgainstDefault(integrityId);
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(scope);
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstNull(window);

        var integrity = new IntelligenceIntegrityAggregate();
        integrity.Apply(new IntelligenceIntegrityEvaluatedEvent(
            integrityId,
            identityId,
            walletId,
            scope.Value,
            integrityScore,
            calibratedConfidence,
            conflictDetected,
            conflictReason,
            window.WindowStart,
            window.WindowEnd,
            window.ObservedAt,
            correlationId)
        {
            CorrelationId = new CorrelationId(correlationId)
        });

        integrity.WalletId = walletId;
        integrity.Scope = scope;
        integrity.IntegrityScore = integrityScore;
        integrity.CalibratedConfidence = calibratedConfidence;
        integrity.ConflictDetected = conflictDetected;
        integrity.ConflictReason = conflictReason;
        integrity.CorrelationId = correlationId;
        integrity.Window = window;
        integrity.Status = EconomicIntelligenceStatus.Completed;

        return integrity;
    }

    private void Apply(IntelligenceIntegrityEvaluatedEvent e)
    {
        Id = e.IntegrityId;
        IdentityId = e.IdentityId;
        RaiseDomainEvent(e);
    }
}

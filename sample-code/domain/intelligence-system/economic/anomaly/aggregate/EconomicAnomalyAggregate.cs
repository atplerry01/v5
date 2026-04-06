namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed class EconomicAnomalyAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid? WalletId { get; private set; }
    public decimal Deviation { get; private set; }
    public decimal DeviationPercentage { get; private set; }
    public SeverityLevel Severity { get; private set; } = default!;
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    // Scope
    public AnalysisScope Scope { get; private set; } = AnalysisScope.Identity;

    // Confidence
    public ConfidenceScore ConfidenceScore { get; private set; } = ConfidenceScore.Zero;

    private EconomicAnomalyAggregate() { }

    public static EconomicAnomalyAggregate Create(
        Guid anomalyId,
        Guid identityId,
        Guid? walletId,
        AnalysisScope scope,
        decimal deviation,
        decimal deviationPercentage,
        SeverityLevel severity,
        ConfidenceScore confidenceScore,
        string correlationId,
        string sourceEventId,
        ObservationWindow window)
    {
        Guard.AgainstDefault(anomalyId);
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(scope);
        Guard.AgainstNull(severity);
        Guard.AgainstNull(confidenceScore);
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstEmpty(sourceEventId);
        Guard.AgainstNull(window);

        var anomaly = new EconomicAnomalyAggregate();
        anomaly.Apply(new EconomicAnomalyDetectedEvent(
            anomalyId,
            identityId,
            walletId,
            scope.Value,
            deviation,
            deviationPercentage,
            severity.Value,
            confidenceScore.Value,
            window.WindowStart,
            window.WindowEnd,
            window.ObservedAt,
            correlationId,
            sourceEventId)
        {
            CorrelationId = new CorrelationId(correlationId)
        });

        anomaly.WalletId = walletId;
        anomaly.Scope = scope;
        anomaly.Deviation = deviation;
        anomaly.DeviationPercentage = deviationPercentage;
        anomaly.ConfidenceScore = confidenceScore;
        anomaly.CorrelationId = correlationId;
        anomaly.SourceEventId = sourceEventId;
        anomaly.Window = window;

        return anomaly;
    }

    private void Apply(EconomicAnomalyDetectedEvent e)
    {
        Id = e.AnomalyId;
        IdentityId = e.IdentityId;
        Severity = new SeverityLevel(e.Severity);
        Status = EconomicIntelligenceStatus.Completed;
        RaiseDomainEvent(e);
    }
}

namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed class EconomicAnalysisAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid? WalletId { get; private set; }
    public Volume TotalVolume { get; private set; } = Volume.Zero;
    public TransactionCount TransactionCount { get; private set; } = TransactionCount.Zero;
    public Velocity Velocity { get; private set; } = Velocity.Zero;
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    // Scope
    public AnalysisScope Scope { get; private set; } = AnalysisScope.Identity;

    private EconomicAnalysisAggregate() { }

    public static EconomicAnalysisAggregate Create(
        Guid analysisId,
        Guid identityId,
        Guid? walletId,
        AnalysisScope scope,
        Volume volume,
        Velocity velocity,
        TransactionCount transactionCount,
        string correlationId,
        string sourceEventId,
        ObservationWindow window)
    {
        Guard.AgainstDefault(analysisId);
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(scope);
        Guard.AgainstNull(volume);
        Guard.AgainstNull(velocity);
        Guard.AgainstNull(transactionCount);
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstEmpty(sourceEventId);
        Guard.AgainstNull(window);

        var analysis = new EconomicAnalysisAggregate();
        analysis.Apply(new EconomicAnalyzedEvent(
            analysisId,
            identityId,
            walletId,
            scope.Value,
            volume.Value,
            velocity.TransactionsPerSecond,
            transactionCount.Value,
            window.WindowStart,
            window.WindowEnd,
            window.ObservedAt,
            correlationId,
            sourceEventId)
        {
            CorrelationId = new CorrelationId(correlationId)
        });

        analysis.TotalVolume = volume;
        analysis.TransactionCount = transactionCount;
        analysis.Velocity = velocity;
        analysis.CorrelationId = correlationId;
        analysis.SourceEventId = sourceEventId;
        analysis.Window = window;
        analysis.Scope = scope;
        analysis.Status = EconomicIntelligenceStatus.Completed;

        return analysis;
    }

    public decimal AverageTransactionSize =>
        TransactionCount.Value > 0
            ? TotalVolume.Value / TransactionCount.Value
            : 0m;

    private void Apply(EconomicAnalyzedEvent e)
    {
        Id = e.AnalysisId;
        IdentityId = e.IdentityId;
        WalletId = e.WalletId;
        RaiseDomainEvent(e);
    }
}

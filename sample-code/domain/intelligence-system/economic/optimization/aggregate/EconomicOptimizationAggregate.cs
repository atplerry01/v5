namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed class EconomicOptimizationAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid? WalletId { get; private set; }
    public RecommendationType RecommendationType { get; private set; } = default!;
    public string SuggestedAction { get; private set; } = string.Empty;
    public ImpactEstimate ExpectedImpact { get; private set; } = default!;
    public ConfidenceScore ConfidenceScore { get; private set; } = ConfidenceScore.Zero;
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    // Scope
    public AnalysisScope Scope { get; private set; } = AnalysisScope.Identity;

    private EconomicOptimizationAggregate() { }

    public static EconomicOptimizationAggregate Create(
        Guid optimizationId,
        Guid identityId,
        Guid? walletId,
        AnalysisScope scope,
        RecommendationType recommendationType,
        string suggestedAction,
        ImpactEstimate expectedImpact,
        ConfidenceScore confidenceScore,
        string correlationId,
        string sourceEventId,
        ObservationWindow window)
    {
        Guard.AgainstDefault(optimizationId);
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(scope);
        Guard.AgainstNull(recommendationType);
        Guard.AgainstEmpty(suggestedAction);
        Guard.AgainstNull(expectedImpact);
        Guard.AgainstNull(confidenceScore);
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstEmpty(sourceEventId);
        Guard.AgainstNull(window);

        var optimization = new EconomicOptimizationAggregate();
        optimization.Apply(new EconomicRecommendationGeneratedEvent(
            optimizationId,
            identityId,
            walletId,
            scope.Value,
            recommendationType.Value,
            suggestedAction,
            expectedImpact.Value,
            confidenceScore.Value,
            window.WindowStart,
            window.WindowEnd,
            window.ObservedAt,
            correlationId,
            sourceEventId)
        {
            CorrelationId = new CorrelationId(correlationId)
        });

        optimization.WalletId = walletId;
        optimization.Scope = scope;
        optimization.CorrelationId = correlationId;
        optimization.SourceEventId = sourceEventId;
        optimization.Window = window;

        return optimization;
    }

    private void Apply(EconomicRecommendationGeneratedEvent e)
    {
        Id = e.OptimizationId;
        IdentityId = e.IdentityId;
        RecommendationType = new RecommendationType(e.RecommendationType);
        SuggestedAction = e.SuggestedAction;
        ExpectedImpact = new ImpactEstimate(e.ExpectedImpact);
        ConfidenceScore = new ConfidenceScore(e.ConfidenceScore);
        Status = EconomicIntelligenceStatus.Completed;
        RaiseDomainEvent(e);
    }
}

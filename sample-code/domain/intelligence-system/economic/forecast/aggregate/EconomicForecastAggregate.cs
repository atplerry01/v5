namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed class EconomicForecastAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid? WalletId { get; private set; }
    public ForecastType ForecastType { get; private set; } = default!;
    public decimal PredictedValue { get; private set; }
    public ConfidenceScore ConfidenceScore { get; private set; } = ConfidenceScore.Zero;
    public TimeHorizon TimeHorizon { get; private set; } = default!;
    public EconomicIntelligenceStatus Status { get; private set; } = EconomicIntelligenceStatus.Pending;

    // Traceability
    public string CorrelationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;

    // Time context
    public ObservationWindow? Window { get; private set; }

    // Scope
    public AnalysisScope Scope { get; private set; } = AnalysisScope.Identity;

    private EconomicForecastAggregate() { }

    public static EconomicForecastAggregate Create(
        Guid forecastId,
        Guid identityId,
        Guid? walletId,
        AnalysisScope scope,
        ForecastType forecastType,
        TimeHorizon timeHorizon,
        decimal predictedValue,
        ConfidenceScore confidenceScore,
        string correlationId,
        string sourceEventId,
        ObservationWindow window)
    {
        Guard.AgainstDefault(forecastId);
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(scope);
        Guard.AgainstNull(forecastType);
        Guard.AgainstNull(timeHorizon);
        Guard.AgainstNull(confidenceScore);
        Guard.AgainstEmpty(correlationId);
        Guard.AgainstEmpty(sourceEventId);
        Guard.AgainstNull(window);

        var forecast = new EconomicForecastAggregate();
        forecast.Apply(new EconomicForecastGeneratedEvent(
            forecastId,
            identityId,
            walletId,
            scope.Value,
            forecastType.Value,
            timeHorizon.Value,
            predictedValue,
            confidenceScore.Value,
            window.WindowStart,
            window.WindowEnd,
            window.ObservedAt,
            correlationId,
            sourceEventId)
        {
            CorrelationId = new CorrelationId(correlationId)
        });

        forecast.WalletId = walletId;
        forecast.Scope = scope;
        forecast.PredictedValue = predictedValue;
        forecast.ConfidenceScore = confidenceScore;
        forecast.CorrelationId = correlationId;
        forecast.SourceEventId = sourceEventId;
        forecast.Window = window;
        forecast.Status = EconomicIntelligenceStatus.Completed;

        return forecast;
    }

    private void Apply(EconomicForecastGeneratedEvent e)
    {
        Id = e.ForecastId;
        IdentityId = e.IdentityId;
        ForecastType = new ForecastType(e.ForecastType);
        TimeHorizon = new TimeHorizon(e.TimeHorizon);
        RaiseDomainEvent(e);
    }
}

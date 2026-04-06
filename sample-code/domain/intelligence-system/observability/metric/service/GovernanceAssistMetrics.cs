namespace Whycespace.Domain.IntelligenceSystem.Observability.Metric;

/// <summary>
/// Governance Assist (AGA) observability metrics.
/// No business logic — observability only.
/// </summary>
public sealed class GovernanceAssistMetrics
{
    private long _recommendationsGenerated;
    private long _recommendationsAccepted;
    private long _recommendationsRejected;
    private long _totalConfidenceSum;
    private long _confidenceCount;
    private long _optimizationAttempts;
    private long _optimizationSuccesses;
    private long _proposalDraftsGenerated;
    private long _analysisCount;
    private long _totalAnalysisLatencyMs;

    // E14.1 hardening metrics
    private long _totalTrustScoreSum;
    private long _trustScoreCount;
    private long _biasDetectedCount;
    private long _guardrailViolationCount;
    private long _feedbackRecordedCount;
    private long _reliabilityCheckCount;

    public void RecordRecommendationGenerated(double confidence)
    {
        Interlocked.Increment(ref _recommendationsGenerated);
        Interlocked.Add(ref _totalConfidenceSum, (long)(confidence * 1000));
        Interlocked.Increment(ref _confidenceCount);
    }

    public void RecordRecommendationAccepted() => Interlocked.Increment(ref _recommendationsAccepted);
    public void RecordRecommendationRejected() => Interlocked.Increment(ref _recommendationsRejected);

    public void RecordOptimization(bool success)
    {
        Interlocked.Increment(ref _optimizationAttempts);
        if (success) Interlocked.Increment(ref _optimizationSuccesses);
    }

    public void RecordProposalDraftGenerated(int count = 1) =>
        Interlocked.Add(ref _proposalDraftsGenerated, count);

    public void RecordAnalysis(long latencyMs)
    {
        Interlocked.Increment(ref _analysisCount);
        Interlocked.Add(ref _totalAnalysisLatencyMs, latencyMs);
    }

    public void RecordTrustScore(double trustScore)
    {
        Interlocked.Add(ref _totalTrustScoreSum, (long)(trustScore * 1000));
        Interlocked.Increment(ref _trustScoreCount);
    }

    public void RecordBiasDetected() => Interlocked.Increment(ref _biasDetectedCount);
    public void RecordGuardrailViolation(int count = 1) => Interlocked.Add(ref _guardrailViolationCount, count);
    public void RecordFeedback() => Interlocked.Increment(ref _feedbackRecordedCount);
    public void RecordReliabilityCheck() => Interlocked.Increment(ref _reliabilityCheckCount);

    public GovernanceAssistMetricsSnapshot GetSnapshot() => new(
        RecommendationsGenerated: Interlocked.Read(ref _recommendationsGenerated),
        RecommendationsAccepted: Interlocked.Read(ref _recommendationsAccepted),
        RecommendationsRejected: Interlocked.Read(ref _recommendationsRejected),
        AverageConfidence: Interlocked.Read(ref _confidenceCount) > 0
            ? (double)Interlocked.Read(ref _totalConfidenceSum) / Interlocked.Read(ref _confidenceCount) / 1000.0
            : 0.0,
        OptimizationAttempts: Interlocked.Read(ref _optimizationAttempts),
        OptimizationSuccesses: Interlocked.Read(ref _optimizationSuccesses),
        OptimizationSuccessRate: Interlocked.Read(ref _optimizationAttempts) > 0
            ? (double)Interlocked.Read(ref _optimizationSuccesses) / Interlocked.Read(ref _optimizationAttempts)
            : 0.0,
        ProposalDraftsGenerated: Interlocked.Read(ref _proposalDraftsGenerated),
        AnalysisCount: Interlocked.Read(ref _analysisCount),
        AverageAnalysisLatencyMs: Interlocked.Read(ref _analysisCount) > 0
            ? (double)Interlocked.Read(ref _totalAnalysisLatencyMs) / Interlocked.Read(ref _analysisCount)
            : 0.0,
        AverageTrustScore: Interlocked.Read(ref _trustScoreCount) > 0
            ? (double)Interlocked.Read(ref _totalTrustScoreSum) / Interlocked.Read(ref _trustScoreCount) / 1000.0
            : 0.0,
        BiasDetectedCount: Interlocked.Read(ref _biasDetectedCount),
        GuardrailViolationCount: Interlocked.Read(ref _guardrailViolationCount),
        FeedbackRecordedCount: Interlocked.Read(ref _feedbackRecordedCount),
        ReliabilityCheckCount: Interlocked.Read(ref _reliabilityCheckCount));
}

public sealed record GovernanceAssistMetricsSnapshot(
    long RecommendationsGenerated,
    long RecommendationsAccepted,
    long RecommendationsRejected,
    double AverageConfidence,
    long OptimizationAttempts,
    long OptimizationSuccesses,
    double OptimizationSuccessRate,
    long ProposalDraftsGenerated,
    long AnalysisCount,
    double AverageAnalysisLatencyMs,
    double AverageTrustScore,
    long BiasDetectedCount,
    long GuardrailViolationCount,
    long FeedbackRecordedCount,
    long ReliabilityCheckCount);

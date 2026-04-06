namespace Whyce.Engines.T0U.WhyceId.Result;

public sealed record EvaluateTrustScoreResult(int TrustScore, string TrustHash, string[] TrustFactors);

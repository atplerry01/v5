namespace Whycespace.Engines.T0U.WhyceId.Trust;

public sealed class TrustScoreEngine : IdentityEngineBase
{
    public TrustScoreResult Compute(ComputeTrustScoreCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new TrustScoreResult(command.SubjectId, 1.0m);
    }
}

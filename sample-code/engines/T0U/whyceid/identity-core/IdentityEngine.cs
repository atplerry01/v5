namespace Whycespace.Engines.T0U.WhyceId.IdentityCore;

public sealed class IdentityEngine : IdentityEngineBase
{
    public IdentityResult Verify(VerifyIdentityCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new IdentityResult(true, command.SubjectId);
    }
}

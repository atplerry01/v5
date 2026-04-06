namespace Whycespace.Engines.T0U.WhyceId.Verification;

public sealed class VerificationHandler : IVerificationEngine
{
    public VerificationDecisionResult Evaluate(VerificationDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CurrentAttempts >= command.MaxAttempts)
            return VerificationDecisionResult.Reject("Maximum verification attempts reached.");

        if (string.IsNullOrWhiteSpace(command.VerificationType))
            return VerificationDecisionResult.Reject("VerificationType is required.");

        return VerificationDecisionResult.Proceed();
    }
}

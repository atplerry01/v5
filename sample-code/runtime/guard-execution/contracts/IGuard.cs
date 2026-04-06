using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Contracts;

public interface IGuard
{
    string Name { get; }
    GuardCategory Category { get; }
    GuardPhase Phase { get; }
    Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default);
}

public enum GuardCategory
{
    Structural,
    Behavioral,
    Domain,
    Engine,
    Runtime,
    Policy,
    PolicyBinding,
    Projection,
    Kafka,
    PromptContainer,
    Systems,
    Test
}

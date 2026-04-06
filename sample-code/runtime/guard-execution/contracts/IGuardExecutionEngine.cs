using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Contracts;

public interface IGuardExecutionEngine
{
    Task<GuardExecutionReport> ExecuteAllAsync(GuardContext context, CancellationToken cancellationToken = default);
    Task<GuardExecutionReport> ExecutePhaseAsync(GuardContext context, GuardPhase phase, CancellationToken cancellationToken = default);
    Task<GuardResult> ExecuteGuardAsync(string guardName, GuardContext context, CancellationToken cancellationToken = default);
    IReadOnlyList<string> RegisteredGuards { get; }
}

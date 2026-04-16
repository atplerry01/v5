using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Decorates an inner middleware and records its name in the StageRecorder
/// each time it executes. Used by ExecutionOrderTest to witness the locked
/// 8-middleware order at runtime.
///
/// The decorator delegates to the real middleware — order is recorded against
/// the actual production code path, not a fake.
/// </summary>
public sealed class RecordingMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly string _name;
    private readonly StageRecorder _recorder;

    public RecordingMiddleware(IMiddleware inner, string name, StageRecorder recorder)
    {
        _inner = inner;
        _name = name;
        _recorder = recorder;
    }

    // phase1.5-S5.2.5 / TB-1: aligned with the post-TC-1 IMiddleware
    // contract that threads CancellationToken through the pipeline
    // (next is now Func<CancellationToken, Task<CommandResult>>).
    public Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        _recorder.Record(_name);
        return _inner.ExecuteAsync(context, command, next, cancellationToken);
    }
}

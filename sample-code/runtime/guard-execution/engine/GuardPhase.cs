namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Determines when a guard executes in the middleware pipeline.
/// PrePolicy guards run before WHYCEPOLICY evaluation.
/// PostPolicy guards run after policy decision is available.
/// </summary>
public enum GuardPhase
{
    PrePolicy = 0,
    PostPolicy = 1
}

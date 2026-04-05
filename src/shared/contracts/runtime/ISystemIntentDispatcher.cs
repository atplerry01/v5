namespace Whyce.Shared.Contracts.Runtime;

public interface ISystemIntentDispatcher
{
    Task<CommandResult> DispatchAsync(object command);
}

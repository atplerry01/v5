namespace Whycespace.Shared.Contracts.Engine;

public interface IEngine
{
    Task ExecuteAsync(IEngineContext context);
}

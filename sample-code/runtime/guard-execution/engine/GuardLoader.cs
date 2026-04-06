using Whycespace.Runtime.GuardExecution.Contracts;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed class GuardLoader
{
    private readonly GuardRegistry _registry;

    public GuardLoader(GuardRegistry registry)
    {
        _registry = registry;
    }

    public void LoadAll(IEnumerable<IGuard> guards)
    {
        foreach (var guard in guards)
        {
            _registry.Register(guard);
        }
    }

    public void LoadFromAssembly(System.Reflection.Assembly assembly)
    {
        var guardTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IGuard).IsAssignableFrom(t)
                        && t.GetConstructor(Type.EmptyTypes) is not null);

        foreach (var type in guardTypes)
        {
            if (Activator.CreateInstance(type) is IGuard guard)
                _registry.Register(guard);
        }
    }
}

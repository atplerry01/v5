using System.Collections.Concurrent;
using Whycespace.Runtime.GuardExecution.Contracts;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed class GuardRegistry
{
    private readonly ConcurrentDictionary<string, IGuard> _guards = new(StringComparer.OrdinalIgnoreCase);
    private volatile bool _isLocked;

    public void Register(IGuard guard)
    {
        if (_isLocked)
            throw new InvalidOperationException("Guard registry is locked. Cannot register new guards after engine initialization.");

        if (!_guards.TryAdd(guard.Name, guard))
            throw new InvalidOperationException($"Guard '{guard.Name}' is already registered.");
    }

    public IGuard? Resolve(string name) =>
        _guards.TryGetValue(name, out var guard) ? guard : null;

    public IReadOnlyList<IGuard> ResolveAll() =>
        _guards.Values.ToList();

    public IReadOnlyList<IGuard> ResolveByCategory(GuardCategory category) =>
        _guards.Values.Where(g => g.Category == category).ToList();

    public IReadOnlyList<IGuard> ResolveByPhase(GuardPhase phase) =>
        _guards.Values.Where(g => g.Phase == phase).ToList();

    public IReadOnlyList<string> RegisteredNames =>
        _guards.Keys.ToList();

    public void Lock() => _isLocked = true;
}

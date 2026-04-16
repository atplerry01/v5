using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Policy;

/// <summary>
/// Default <see cref="ICommandPolicyIdRegistry"/> implementation. Aggregates
/// every <see cref="CommandPolicyBinding"/> registered in DI into an immutable
/// type-keyed lookup at construction time. Falls back to <see cref="Default"/>
/// for any command type without an explicit binding so untyped flows continue
/// to resolve while contexts are being onboarded.
///
/// Construction-time validation rejects duplicate bindings for the same
/// command type — a duplicate is always a wiring bug (two modules claim the
/// same command) and must fail loud rather than silently shadow.
/// </summary>
public sealed class CommandPolicyIdRegistry : ICommandPolicyIdRegistry
{
    public const string Default = "whyce-policy-default";

    private readonly IReadOnlyDictionary<Type, string> _map;

    public CommandPolicyIdRegistry(IEnumerable<CommandPolicyBinding> bindings)
    {
        var map = new Dictionary<Type, string>();
        foreach (var b in bindings)
        {
            if (map.TryGetValue(b.CommandType, out var existing))
                throw new InvalidOperationException(
                    $"Duplicate CommandPolicyBinding for {b.CommandType.FullName}: " +
                    $"existing='{existing}', incoming='{b.PolicyId}'. " +
                    "Each command may bind to exactly one policy id.");
            map[b.CommandType] = b.PolicyId;
        }
        _map = map;
    }

    public string Resolve(Type commandType) =>
        _map.TryGetValue(commandType, out var id) ? id : Default;
}

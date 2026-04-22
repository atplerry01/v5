using Whycespace.Domain.CoreSystem.Ordering.OrderingKey;

namespace Whycespace.Domain.CoreSystem.Ordering.OrderingRule;

public sealed record OrderingRule
{
    private const int MaxChainDepth = 8;

    public OrderingKey.OrderingKey Key { get; }
    public OrderingDirection Direction { get; }
    public OrderingRule? TieBreaker { get; }

    public OrderingRule(
        OrderingKey.OrderingKey key,
        OrderingDirection direction,
        OrderingRule? tieBreaker = null)
    {
        if (tieBreaker is not null && tieBreaker.Key.Value == key.Value)
            throw OrderingRuleErrors.TieBreakerKeyMustDiffer(key);

        var depth = 0;
        var current = tieBreaker;
        while (current is not null)
        {
            depth++;
            if (depth >= MaxChainDepth)
                throw OrderingRuleErrors.TieBreakerChainExceedsMaxDepth(MaxChainDepth);
            current = current.TieBreaker;
        }

        Key = key;
        Direction = direction;
        TieBreaker = tieBreaker;
    }

    public static OrderingRule AscendingBy(OrderingKey.OrderingKey key) =>
        new(key, OrderingDirection.Ascending);

    public static OrderingRule DescendingBy(OrderingKey.OrderingKey key) =>
        new(key, OrderingDirection.Descending);

    public OrderingRule WithTieBreaker(OrderingRule tieBreaker) =>
        new(Key, Direction, tieBreaker);

    // Compares two keys under this rule, falling through to TieBreaker on equality.
    public int Compare(OrderingKey.OrderingKey a, OrderingKey.OrderingKey b)
    {
        var result = Direction == OrderingDirection.Ascending
            ? a.CompareTo(b)
            : b.CompareTo(a);

        if (result != 0)
            return result;

        return TieBreaker?.Compare(a, b) ?? 0;
    }

    public override string ToString() =>
        TieBreaker is null
            ? $"{Direction}({Key})"
            : $"{Direction}({Key}) then {TieBreaker}";
}

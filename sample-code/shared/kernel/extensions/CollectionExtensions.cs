namespace Whycespace.Shared.Kernel.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
        source is null || !source.Any();

    public static IReadOnlyList<T> OrEmpty<T>(this IReadOnlyList<T>? source) =>
        source ?? [];

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class =>
        source.Where(x => x is not null)!;
}

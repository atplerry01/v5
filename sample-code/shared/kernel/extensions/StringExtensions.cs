namespace Whycespace.Shared.Kernel.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) =>
        string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    public static string ToSnakeCase(this string value) =>
        string.Concat(value.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? $"_{c}" : $"{c}"))
            .ToLowerInvariant();
}

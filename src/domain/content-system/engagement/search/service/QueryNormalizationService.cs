namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public static class QueryNormalizationService
{
    public static string Normalise(string text)
    {
        if (text is null) return string.Empty;
        var lowered = text.Trim().ToLowerInvariant();
        var builder = new System.Text.StringBuilder(lowered.Length);
        var lastWasSpace = true;
        foreach (var c in lowered)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasSpace = false;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace)
                {
                    builder.Append(' ');
                    lastWasSpace = true;
                }
            }
        }
        return builder.ToString().TrimEnd();
    }

    public static IReadOnlyList<string> Tokenise(string text)
    {
        var normalised = Normalise(text);
        if (normalised.Length == 0) return Array.Empty<string>();
        return normalised.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}

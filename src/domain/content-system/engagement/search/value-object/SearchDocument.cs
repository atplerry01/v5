using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public sealed record SearchDocument : ValueObject
{
    public string DocumentRef { get; }
    public string NormalisedText { get; }

    private SearchDocument(string documentRef, string normalisedText)
    {
        DocumentRef = documentRef;
        NormalisedText = normalisedText;
    }

    public static SearchDocument Create(string documentRef, string text)
    {
        if (string.IsNullOrWhiteSpace(documentRef)) throw SearchErrors.InvalidDocument();
        var norm = QueryNormalizationService.Normalise(text ?? string.Empty);
        return new SearchDocument(documentRef, norm);
    }
}

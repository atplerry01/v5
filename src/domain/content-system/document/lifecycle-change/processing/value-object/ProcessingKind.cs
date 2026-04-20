namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public enum ProcessingKind
{
    Ocr,
    TextExtraction,
    PreviewGeneration,
    Rendering,
    FormatConversion,
    Redaction,
    IndexingPreparation
}

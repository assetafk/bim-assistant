namespace BimAiAssistant.Models;

public sealed class ExportResult
{
    public ExportFormat Format { get; set; }
    public string Path { get; set; } = string.Empty;
}

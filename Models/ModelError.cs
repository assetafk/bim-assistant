namespace BimAiAssistant.Models;

public sealed class ModelError
{
    public string ElementType { get; set; } = string.Empty;
    public long ElementId { get; set; }
    public string Message { get; set; } = string.Empty;
}

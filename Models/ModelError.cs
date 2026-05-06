namespace BimAiAssistant.Models;

public sealed class ModelError
{
    public string RuleId { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
    public string ElementType { get; set; } = string.Empty;
    public long ElementId { get; set; }
    public string Message { get; set; } = string.Empty;
}

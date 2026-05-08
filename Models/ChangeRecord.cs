namespace BimAiAssistant.Models;

public sealed class ChangeRecord
{
    public long ElementId { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
}

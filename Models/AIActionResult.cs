namespace BimAiAssistant.Models;

public sealed class AIActionResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int AffectedElements { get; set; }
    public string OperationId { get; set; } = string.Empty;
}

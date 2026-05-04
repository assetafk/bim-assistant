namespace BimAiAssistant.Models;

public sealed class AIActionRequest
{
    public AIActionType Type { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public Dictionary<string, string> Arguments { get; set; } = [];
}

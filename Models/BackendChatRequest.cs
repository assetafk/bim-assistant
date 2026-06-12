namespace BimAiAssistant.Models;

public sealed class BackendChatRequest
{
    public string Message { get; set; } = string.Empty;
    public BuildingModel Model { get; set; } = new();
}

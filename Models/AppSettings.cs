namespace BimAiAssistant.Models;

public sealed class AppSettings
{
    public string ApiUrl { get; set; } = "http://localhost:11434/api/chat";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "llama3.1";
    public double Temperature { get; set; } = 0.2;
}

namespace BimAiAssistant.Models;

public sealed class ValidationRule
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Check { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
    public bool Enabled { get; set; } = true;
}

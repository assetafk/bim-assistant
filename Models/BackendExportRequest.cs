namespace BimAiAssistant.Models;

public sealed class BackendExportRequest
{
    public string Project { get; set; } = string.Empty;
    public ExportFormat Format { get; set; }
    public BuildingModel Model { get; set; } = new();
}

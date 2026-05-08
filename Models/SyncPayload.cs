namespace BimAiAssistant.Models;

public sealed class SyncPayload
{
    public string Organization { get; set; } = "Maybeworks";
    public string Project { get; set; } = string.Empty;
    public DateTimeOffset SyncedAt { get; set; } = DateTimeOffset.UtcNow;
    public BuildingModel Model { get; set; } = new();
    public IReadOnlyList<ModelError> ValidationIssues { get; set; } = [];
    public IReadOnlyList<ChangeOperation> ChangeHistory { get; set; } = [];
}

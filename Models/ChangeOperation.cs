namespace BimAiAssistant.Models;

public sealed class ChangeOperation
{
    public string OperationId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserName { get; set; } = "LocalUser";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string ActionName { get; set; } = string.Empty;
    public string Source { get; set; } = "RevitPlugin";
    public List<ChangeRecord> Changes { get; set; } = [];
    public bool IsUndone { get; set; }
}

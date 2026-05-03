namespace BimAiAssistant.Models;

public sealed class ColumnModel
{
    public long Id { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
}

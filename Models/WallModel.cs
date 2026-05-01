namespace BimAiAssistant.Models;

public sealed class WallModel
{
    public long Id { get; set; }
    public double Length { get; set; }
    public double Height { get; set; }
    public string Material { get; set; } = string.Empty;
}

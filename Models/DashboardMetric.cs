namespace BimAiAssistant.Models;

public sealed class DashboardMetric
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public double BarWidth { get; set; }
    public string Color { get; set; } = "#2563EB";
}

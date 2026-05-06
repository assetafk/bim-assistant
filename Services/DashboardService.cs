using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class DashboardService
{
    public IReadOnlyList<DashboardMetric> BuildMetrics(BuildingModel model, IReadOnlyList<ModelError> errors)
    {
        int materials = model.Walls
            .Select(wall => wall.Material)
            .Concat(model.Columns.Select(column => column.Material))
            .Where(material => !string.IsNullOrWhiteSpace(material))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var metrics = new List<DashboardMetric>
        {
            new() { Name = "Errors", Value = errors.Count, Color = "#DC2626" },
            new() { Name = "Rooms", Value = model.Rooms.Count, Color = "#2563EB" },
            new() { Name = "Levels", Value = model.Levels.Count, Color = "#7C3AED" },
            new() { Name = "Doors", Value = model.Doors.Count, Color = "#059669" },
            new() { Name = "Windows", Value = model.Windows.Count, Color = "#0891B2" },
            new() { Name = "Materials", Value = materials, Color = "#D97706" }
        };

        int max = Math.Max(1, metrics.Max(metric => metric.Value));
        foreach (DashboardMetric metric in metrics)
        {
            metric.BarWidth = Math.Max(8, metric.Value * 420d / max);
        }

        return metrics;
    }
}

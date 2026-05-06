namespace BimAiAssistant.Models;

public sealed class AIReportSection
{
    public IReadOnlyList<string> Recommendations { get; set; } = [];
    public IReadOnlyList<string> ProblemZones { get; set; } = [];
}

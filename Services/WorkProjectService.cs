using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class WorkProjectService
{
    public IReadOnlyList<WorkProject> GetAvailableProjects() =>
    [
        new()
        {
            Name = "Business Center",
            Code = "MW-BC",
            Description = "Commercial office building BIM workflow."
        },
        new()
        {
            Name = "Shopping Mall",
            Code = "MW-SM",
            Description = "Retail complex BIM coordination workflow."
        },
        new()
        {
            Name = "School",
            Code = "MW-SC",
            Description = "Education facility BIM documentation workflow."
        },
        new()
        {
            Name = "Hospital",
            Code = "MW-HP",
            Description = "Healthcare facility BIM validation workflow."
        }
    ];
}

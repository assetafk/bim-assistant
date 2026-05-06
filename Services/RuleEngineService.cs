using BimAiAssistant.Models;
using Newtonsoft.Json;
using System.IO;

namespace BimAiAssistant.Services;

public sealed class RuleEngineService
{
    private readonly string _rulesPath;

    public RuleEngineService()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BimAiAssistant");

        Directory.CreateDirectory(directory);
        _rulesPath = Path.Combine(directory, "rules.json");
    }

    public IReadOnlyList<ValidationRule> LoadRules()
    {
        if (!File.Exists(_rulesPath))
        {
            SaveRules(GetDefaultRules());
        }

        try
        {
            return JsonConvert.DeserializeObject<List<ValidationRule>>(File.ReadAllText(_rulesPath)) ?? [];
        }
        catch
        {
            return GetDefaultRules();
        }
    }

    public void SaveRules(IReadOnlyList<ValidationRule> rules)
    {
        File.WriteAllText(_rulesPath, JsonConvert.SerializeObject(rules, Formatting.Indented));
    }

    private static List<ValidationRule> GetDefaultRules() =>
    [
        new()
        {
            Id = "MW-WALL-FIRE-RATING",
            Name = "All walls must have FireRating",
            Category = "Walls",
            Check = "RequiredParameter",
            ParameterName = "FireRating",
            Severity = "Error"
        },
        new()
        {
            Id = "MW-DOOR-WIDTH",
            Name = "Doors must have Width",
            Category = "Doors",
            Check = "RequiredParameter",
            ParameterName = "Width",
            Severity = "Error"
        }
    ];
}

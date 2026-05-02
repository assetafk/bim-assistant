using BimAiAssistant.Models;
using Newtonsoft.Json;
using System.IO;

namespace BimAiAssistant.Services;

public sealed class ExportService
{
    public string ExportBuilding(BuildingModel model)
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BimAiAssistant");

        Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, "building.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(model, Formatting.Indented));
        return path;
    }
}

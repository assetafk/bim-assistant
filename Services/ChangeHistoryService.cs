using BimAiAssistant.Models;
using Newtonsoft.Json;
using System.IO;

namespace BimAiAssistant.Services;

public sealed class ChangeHistoryService
{
    private readonly string _historyPath;

    public ChangeHistoryService()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BimAiAssistant");

        Directory.CreateDirectory(directory);
        _historyPath = Path.Combine(directory, "change-history.json");
    }

    public IReadOnlyList<ChangeOperation> Load()
    {
        if (!File.Exists(_historyPath))
        {
            return [];
        }

        try
        {
            return JsonConvert.DeserializeObject<List<ChangeOperation>>(File.ReadAllText(_historyPath)) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Append(ChangeOperation operation)
    {
        var operations = Load().ToList();
        operations.Add(operation);
        Save(operations);
    }

    public void MarkUndone(string operationId)
    {
        var operations = Load().ToList();
        ChangeOperation? operation = operations.FirstOrDefault(item => item.OperationId == operationId);
        if (operation is not null)
        {
            operation.IsUndone = true;
            Save(operations);
        }
    }

    private void Save(IReadOnlyList<ChangeOperation> operations)
    {
        File.WriteAllText(_historyPath, JsonConvert.SerializeObject(operations, Formatting.Indented));
    }
}

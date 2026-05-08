using Autodesk.Revit.DB;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class RevitActionService
{
    private readonly Document _document;
    private readonly ChangeHistoryService _changeHistoryService;

    public RevitActionService(Document document)
    {
        _document = document;
        _changeHistoryService = new ChangeHistoryService();
    }

    public AIActionResult Execute(AIActionRequest request)
    {
        return request.Type switch
        {
            AIActionType.RenameDoorsByCompanyStandard => RenameDoorsByCompanyStandard(),
            AIActionType.FillMissingParameters => FillMissingParameters(),
            AIActionType.CreateDoorSchedule => CreateDoorSchedule(),
            _ => new AIActionResult { Succeeded = false, Message = "Unsupported AI action." }
        };
    }

    private AIActionResult RenameDoorsByCompanyStandard()
    {
        IReadOnlyList<Element> doors = GetDoors().ToList();
        int changed = 0;
        var operation = CreateOperation(nameof(AIActionType.RenameDoorsByCompanyStandard));

        using var transaction = new Transaction(_document, "Maybeworks AI: Rename doors");
        transaction.Start();

        foreach (var group in doors.GroupBy(GetLevelName))
        {
            int index = 1;
            string levelCode = NormalizeCode(group.Key, "LVL");

            foreach (Element door in group.OrderBy(item => item.Id.Value))
            {
                Parameter? mark = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (SetString(door, mark, "Mark", $"MW-DR-{levelCode}-{index:000}", operation))
                {
                    changed++;
                }

                index++;
            }
        }

        transaction.Commit();
        SaveOperation(operation);

        return new AIActionResult
        {
            Succeeded = true,
            OperationId = operation.OperationId,
            AffectedElements = changed,
            Message = $"Renamed {changed} doors by Maybeworks company standard."
        };
    }

    private AIActionResult FillMissingParameters()
    {
        int changed = 0;
        var operation = CreateOperation(nameof(AIActionType.FillMissingParameters));

        using var transaction = new Transaction(_document, "Maybeworks AI: Fill missing parameters");
        transaction.Start();

        foreach (Element door in GetDoors())
        {
            Parameter? mark = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
            if (string.IsNullOrWhiteSpace(mark?.AsString()) && SetString(door, mark, "Mark", $"MW-DR-{door.Id.Value}", operation))
            {
                changed++;
            }
        }

        foreach (Wall wall in GetWalls())
        {
            Parameter? comments = wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            string material = GetMaterialName(wall);
            if (string.IsNullOrWhiteSpace(material) && SetString(wall, comments, "Comments", "AI review required: material is missing.", operation))
            {
                changed++;
            }
        }

        transaction.Commit();
        SaveOperation(operation);

        return new AIActionResult
        {
            Succeeded = true,
            OperationId = operation.OperationId,
            AffectedElements = changed,
            Message = $"Filled or flagged {changed} missing parameters."
        };
    }

    private AIActionResult CreateDoorSchedule()
    {
        var operation = CreateOperation(nameof(AIActionType.CreateDoorSchedule));
        using var transaction = new Transaction(_document, "Maybeworks AI: Create door schedule");
        transaction.Start();

        ViewSchedule schedule = ViewSchedule.CreateSchedule(_document, new ElementId((int)BuiltInCategory.OST_Doors));
        schedule.Name = $"Maybeworks Door Schedule {DateTime.Now:yyyyMMdd-HHmm}";
        operation.Changes.Add(new ChangeRecord
        {
            ElementId = schedule.Id.Value,
            ElementType = "ViewSchedule",
            ParameterName = "Name",
            OldValue = string.Empty,
            NewValue = schedule.Name
        });

        transaction.Commit();
        SaveOperation(operation);

        return new AIActionResult
        {
            Succeeded = true,
            OperationId = operation.OperationId,
            AffectedElements = 1,
            Message = $"Created schedule: {schedule.Name}."
        };
    }

    private IEnumerable<Element> GetDoors() =>
        new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType()
            .ToElements();

    private IEnumerable<Wall> GetWalls() =>
        new FilteredElementCollector(_document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>();

    private string GetLevelName(Element element)
    {
        if (_document.GetElement(element.LevelId) is Level level)
        {
            return level.Name;
        }

        return "Unknown";
    }

    private string GetMaterialName(Element element) =>
        element.GetMaterialIds(false)
            .Select(id => _document.GetElement(id))
            .OfType<Material>()
            .Select(material => material.Name)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty;

    private bool SetString(Element element, Parameter? parameter, string parameterName, string value, ChangeOperation operation)
    {
        if (parameter is null || parameter.IsReadOnly)
        {
            return false;
        }

        string oldValue = parameter.AsString() ?? parameter.AsValueString() ?? string.Empty;
        if (oldValue == value)
        {
            return false;
        }

        parameter.Set(value);
        operation.Changes.Add(new ChangeRecord
        {
            ElementId = element.Id.Value,
            ElementType = element.GetType().Name,
            ParameterName = parameterName,
            OldValue = oldValue,
            NewValue = value
        });

        return true;
    }

    private static ChangeOperation CreateOperation(string actionName) => new()
    {
        ActionName = actionName,
        UserName = Environment.UserName,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private void SaveOperation(ChangeOperation operation)
    {
        if (operation.Changes.Count > 0)
        {
            _changeHistoryService.Append(operation);
        }
    }

    private static string NormalizeCode(string value, string fallback)
    {
        string code = new(value.Where(char.IsLetterOrDigit).Take(8).ToArray());
        return string.IsNullOrWhiteSpace(code) ? fallback : code.ToUpperInvariant();
    }
}

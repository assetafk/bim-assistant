using Autodesk.Revit.DB;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class RevitActionService
{
    private readonly Document _document;

    public RevitActionService(Document document)
    {
        _document = document;
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

        using var transaction = new Transaction(_document, "Maybeworks AI: Rename doors");
        transaction.Start();

        foreach (var group in doors.GroupBy(GetLevelName))
        {
            int index = 1;
            string levelCode = NormalizeCode(group.Key, "LVL");

            foreach (Element door in group.OrderBy(item => item.Id.Value))
            {
                Parameter? mark = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (SetString(mark, $"MW-DR-{levelCode}-{index:000}"))
                {
                    changed++;
                }

                index++;
            }
        }

        transaction.Commit();

        return new AIActionResult
        {
            Succeeded = true,
            AffectedElements = changed,
            Message = $"Renamed {changed} doors by Maybeworks company standard."
        };
    }

    private AIActionResult FillMissingParameters()
    {
        int changed = 0;

        using var transaction = new Transaction(_document, "Maybeworks AI: Fill missing parameters");
        transaction.Start();

        foreach (Element door in GetDoors())
        {
            Parameter? mark = door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
            if (string.IsNullOrWhiteSpace(mark?.AsString()) && SetString(mark, $"MW-DR-{door.Id.Value}"))
            {
                changed++;
            }
        }

        foreach (Wall wall in GetWalls())
        {
            Parameter? comments = wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            string material = GetMaterialName(wall);
            if (string.IsNullOrWhiteSpace(material) && SetString(comments, "AI review required: material is missing."))
            {
                changed++;
            }
        }

        transaction.Commit();

        return new AIActionResult
        {
            Succeeded = true,
            AffectedElements = changed,
            Message = $"Filled or flagged {changed} missing parameters."
        };
    }

    private AIActionResult CreateDoorSchedule()
    {
        using var transaction = new Transaction(_document, "Maybeworks AI: Create door schedule");
        transaction.Start();

        ViewSchedule schedule = ViewSchedule.CreateSchedule(_document, new ElementId((int)BuiltInCategory.OST_Doors));
        schedule.Name = $"Maybeworks Door Schedule {DateTime.Now:yyyyMMdd-HHmm}";

        transaction.Commit();

        return new AIActionResult
        {
            Succeeded = true,
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

    private static bool SetString(Parameter? parameter, string value)
    {
        if (parameter is null || parameter.IsReadOnly)
        {
            return false;
        }

        parameter.Set(value);
        return true;
    }

    private static string NormalizeCode(string value, string fallback)
    {
        string code = new(value.Where(char.IsLetterOrDigit).Take(8).ToArray());
        return string.IsNullOrWhiteSpace(code) ? fallback : code.ToUpperInvariant();
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class ValidationService
{
    private readonly Document _document;
    private readonly RuleEngineService _ruleEngineService;

    public ValidationService(Document document, RuleEngineService ruleEngineService)
    {
        _document = document;
        _ruleEngineService = ruleEngineService;
    }

    public IReadOnlyList<ModelError> Validate()
    {
        var errors = new List<ModelError>();

        errors.AddRange(CheckEmptyParameters());
        errors.AddRange(CheckInvalidFamilies());
        errors.AddRange(CheckMissingMaterials());
        errors.AddRange(CheckIntersections());
        errors.AddRange(CheckDuplicates());
        errors.AddRange(CheckUnusedTypes());
        errors.AddRange(CheckInvalidLevels());
        errors.AddRange(ApplyCustomRules(_ruleEngineService.LoadRules()));

        return errors;
    }

    private IEnumerable<ModelError> CheckEmptyParameters()
    {
        foreach (Element door in Elements(BuiltInCategory.OST_Doors))
        {
            if (IsEmpty(door.LookupParameter("Mark")) && IsEmpty(door.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)))
            {
                yield return Error("MW-EMPTY-DOOR-MARK", "Error", "Door", door.Id, "Door mark is empty.");
            }
        }

        foreach (Room room in Rooms())
        {
            if (room.Area <= 0)
            {
                yield return Error("MW-EMPTY-ROOM-AREA", "Error", "Room", room.Id, "Room area is empty.");
            }
        }
    }

    private IEnumerable<ModelError> CheckInvalidFamilies()
    {
        foreach (Family family in Families())
        {
            if (!family.Name.StartsWith("MW_", StringComparison.OrdinalIgnoreCase))
            {
                yield return Error("MW-INVALID-FAMILY-NAME", "Warning", "Family", family.Id, $"Family '{family.Name}' does not follow Maybeworks naming standard.");
            }
        }
    }

    private IEnumerable<ModelError> CheckMissingMaterials()
    {
        foreach (Wall wall in Walls())
        {
            if (!wall.GetMaterialIds(false).Any())
            {
                yield return Error("MW-MISSING-WALL-MATERIAL", "Error", "Wall", wall.Id, "Wall material is missing.");
            }
        }
    }

    private IEnumerable<ModelError> CheckIntersections()
    {
        foreach (Wall wall in Walls())
        {
            var filter = new ElementIntersectsElementFilter(wall);
            bool hasIntersection = new FilteredElementCollector(_document)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .Any(element => element.Id.Value != wall.Id.Value);

            if (hasIntersection)
            {
                yield return Error("MW-WALL-INTERSECTION", "Warning", "Wall", wall.Id, "Wall intersects another wall.");
            }
        }
    }

    private IEnumerable<ModelError> CheckDuplicates()
    {
        foreach (var group in Elements(BuiltInCategory.OST_Doors)
            .Select(door => new { Door = door, Mark = GetParameterValue(door, "Mark") })
            .Where(item => !string.IsNullOrWhiteSpace(item.Mark))
            .GroupBy(item => item.Mark)
            .Where(group => group.Count() > 1))
        {
            foreach (var item in group)
            {
                yield return Error("MW-DUPLICATE-DOOR-MARK", "Error", "Door", item.Door.Id, $"Duplicate door mark '{group.Key}'.");
            }
        }
    }

    private IEnumerable<ModelError> CheckUnusedTypes()
    {
        HashSet<long> usedTypeIds = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .Select(element => element.GetTypeId().Value)
            .Where(id => id > 0)
            .ToHashSet();

        foreach (ElementType type in new FilteredElementCollector(_document).WhereElementIsElementType().OfType<ElementType>())
        {
            if (!usedTypeIds.Contains(type.Id.Value))
            {
                yield return Error("MW-UNUSED-TYPE", "Info", "Type", type.Id, $"Unused type '{type.Name}'.");
            }
        }
    }

    private IEnumerable<ModelError> CheckInvalidLevels()
    {
        foreach (Element element in Elements(BuiltInCategory.OST_Doors).Concat(Elements(BuiltInCategory.OST_Windows)))
        {
            if (element.LevelId == ElementId.InvalidElementId || _document.GetElement(element.LevelId) is not Level)
            {
                yield return Error("MW-INVALID-LEVEL", "Error", element.GetType().Name, element.Id, "Element has invalid or missing level.");
            }
        }
    }

    private IEnumerable<ModelError> ApplyCustomRules(IEnumerable<ValidationRule> rules)
    {
        foreach (ValidationRule rule in rules.Where(rule => rule.Enabled))
        {
            if (!rule.Check.Equals("RequiredParameter", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (Element element in ElementsByRuleCategory(rule.Category))
            {
                if (IsEmpty(element.LookupParameter(rule.ParameterName)))
                {
                    yield return Error(rule.Id, rule.Severity, rule.Category.TrimEnd('s'), element.Id, $"{rule.Name}: parameter '{rule.ParameterName}' is empty.");
                }
            }
        }
    }

    private IEnumerable<Element> ElementsByRuleCategory(string category) =>
        category.ToLowerInvariant() switch
        {
            "walls" => Walls(),
            "doors" => Elements(BuiltInCategory.OST_Doors),
            "windows" => Elements(BuiltInCategory.OST_Windows),
            "columns" => Elements(BuiltInCategory.OST_Columns).Concat(Elements(BuiltInCategory.OST_StructuralColumns)),
            "rooms" => Rooms(),
            _ => []
        };

    private IEnumerable<Wall> Walls() =>
        new FilteredElementCollector(_document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>();

    private IEnumerable<Family> Families() =>
        new FilteredElementCollector(_document)
            .OfClass(typeof(Family))
            .Cast<Family>();

    private IEnumerable<Room> Rooms() =>
        new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>();

    private IEnumerable<Element> Elements(BuiltInCategory category) =>
        new FilteredElementCollector(_document)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToElements();

    private static bool IsEmpty(Parameter? parameter) =>
        parameter is null || string.IsNullOrWhiteSpace(parameter.AsString() ?? parameter.AsValueString());

    private static string GetParameterValue(Element element, string name)
    {
        Parameter? parameter = element.LookupParameter(name);
        return parameter?.AsString() ?? parameter?.AsValueString() ?? string.Empty;
    }

    private static ModelError Error(string ruleId, string severity, string type, ElementId id, string message) => new()
    {
        RuleId = ruleId,
        Severity = severity,
        ElementType = type,
        ElementId = id.Value,
        Message = message
    };
}

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class RevitService
{
    private const double FeetToMillimeters = 304.8;
    private const double SquareFeetToSquareMeters = 0.09290304;
    private readonly Document _document;

    public RevitService(Document document)
    {
        _document = document;
    }

    public ModelStatistics GetStatistics()
    {
        int walls = Count<Wall>();
        int doors = Elements(BuiltInCategory.OST_Doors).Count();
        int windows = Elements(BuiltInCategory.OST_Windows).Count();
        int floors = Count<Floor>();
        List<Room> rooms = Rooms().ToList();
        double area = rooms.Sum(room => UnitUtils.ConvertFromInternalUnits(room.Area, UnitTypeId.SquareMeters));

        return new ModelStatistics(walls, doors, windows, floors, rooms.Count, Math.Round(area, 2));
    }

    public BuildingModel GetBuildingModel()
    {
        var model = new BuildingModel
        {
            Project = string.IsNullOrWhiteSpace(_document.ProjectInformation?.Name)
                ? Path.GetFileNameWithoutExtension(_document.Title)
                : _document.ProjectInformation.Name,
            LevelCount = Count<Level>(),
            Statistics = GetStatistics(),
            Walls = Walls().Select(ToWallModel).ToList(),
            Doors = Elements(BuiltInCategory.OST_Doors).Select(ToDoorModel).ToList(),
            Windows = Elements(BuiltInCategory.OST_Windows).Select(ToWindowModel).ToList(),
            Rooms = Rooms().Select(ToRoomModel).ToList()
        };

        return model;
    }

    public IReadOnlyList<ModelError> FindModelErrors()
    {
        var errors = new List<ModelError>();

        errors.AddRange(Rooms()
            .Where(room => room.Area <= 0)
            .Select(room => Error("Room", room.Id, "Area is empty")));

        errors.AddRange(Elements(BuiltInCategory.OST_Doors)
            .Where(door => string.IsNullOrWhiteSpace(GetString(door, BuiltInParameter.ALL_MODEL_MARK)))
            .Select(door => Error("Door", door.Id, "Mark is empty")));

        errors.AddRange(Walls()
            .Where(wall => string.IsNullOrWhiteSpace(GetWallMaterial(wall)))
            .Select(wall => Error("Wall", wall.Id, "Material is empty")));

        errors.AddRange(Elements(BuiltInCategory.OST_Windows)
            .Where(window => !HasValidLevel(window))
            .Select(window => Error("Window", window.Id, "Window is outside level")));

        return errors;
    }

    private int Count<TElement>() where TElement : Element =>
        new FilteredElementCollector(_document)
            .OfClass(typeof(TElement))
            .WhereElementIsNotElementType()
            .GetElementCount();

    private IEnumerable<Wall> Walls() =>
        new FilteredElementCollector(_document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>();

    private IEnumerable<Element> Elements(BuiltInCategory category) =>
        new FilteredElementCollector(_document)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToElements();

    private IEnumerable<Room> Rooms() =>
        new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>();

    private WallModel ToWallModel(Wall wall) => new()
    {
        Id = wall.Id.Value,
        Length = Math.Round(GetDouble(wall, BuiltInParameter.CURVE_ELEM_LENGTH) * FeetToMillimeters, 2),
        Height = Math.Round(GetDouble(wall, BuiltInParameter.WALL_USER_HEIGHT_PARAM) * FeetToMillimeters, 2),
        Material = GetWallMaterial(wall)
    };

    private DoorModel ToDoorModel(Element door) => new()
    {
        Id = door.Id.Value,
        Mark = GetString(door, BuiltInParameter.ALL_MODEL_MARK),
        Level = GetLevelName(door)
    };

    private WindowModel ToWindowModel(Element window) => new()
    {
        Id = window.Id.Value,
        Level = GetLevelName(window)
    };

    private RoomModel ToRoomModel(Room room) => new()
    {
        Id = room.Id.Value,
        Name = room.Name ?? string.Empty,
        Area = Math.Round(UnitUtils.ConvertFromInternalUnits(room.Area, UnitTypeId.SquareMeters), 2)
    };

    private string GetWallMaterial(Wall wall)
    {
        string material = GetMaterialNameFromParameter(wall, BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
        if (!string.IsNullOrWhiteSpace(material))
        {
            return material;
        }

        return wall.GetMaterialIds(false)
            .Select(id => _document.GetElement(id))
            .OfType<Material>()
            .Select(item => item.Name)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty;
    }

    private string GetMaterialNameFromParameter(Element element, BuiltInParameter parameterId)
    {
        Parameter? parameter = element.get_Parameter(parameterId);
        if (parameter is null || parameter.StorageType != StorageType.ElementId)
        {
            return string.Empty;
        }

        return _document.GetElement(parameter.AsElementId()) is Material material ? material.Name : string.Empty;
    }

    private string GetLevelName(Element element)
    {
        ElementId levelId = element.LevelId;
        if (levelId == ElementId.InvalidElementId)
        {
            Parameter? parameter = element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                ?? element.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
            levelId = parameter?.AsElementId() ?? ElementId.InvalidElementId;
        }

        return _document.GetElement(levelId) is Level level ? level.Name : string.Empty;
    }

    private bool HasValidLevel(Element element) => !string.IsNullOrWhiteSpace(GetLevelName(element));

    private static double GetDouble(Element element, BuiltInParameter parameterId)
    {
        Parameter? parameter = element.get_Parameter(parameterId);
        return parameter?.StorageType == StorageType.Double ? parameter.AsDouble() : 0;
    }

    private static string GetString(Element element, BuiltInParameter parameterId)
    {
        Parameter? parameter = element.get_Parameter(parameterId);
        return parameter?.AsString() ?? parameter?.AsValueString() ?? string.Empty;
    }

    private static ModelError Error(string type, ElementId id, string message) => new()
    {
        ElementType = type,
        ElementId = id.Value,
        Message = message
    };
}

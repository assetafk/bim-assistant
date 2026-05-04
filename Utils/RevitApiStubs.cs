#if REVIT_STUBS
namespace Autodesk.Revit.Attributes
{
    public sealed class TransactionAttribute : Attribute
    {
        public TransactionAttribute(TransactionMode mode) { }
    }

    public enum TransactionMode
    {
        Manual
    }
}

namespace Autodesk.Revit.DB
{
    public class Element
    {
        public ElementId Id { get; init; } = new(0);
        public ElementId LevelId { get; init; } = ElementId.InvalidElementId;
        public string Name { get; set; } = string.Empty;
        public virtual Parameter? get_Parameter(BuiltInParameter parameter) => null;
        public virtual ICollection<ElementId> GetMaterialIds(bool returnPaintMaterials) => [];
        public virtual ElementId GetTypeId() => ElementId.InvalidElementId;
    }

    public class Wall : Element { }
    public class Floor : Element { }
    public class Level : Element { public double Elevation { get; init; } }
    public class Material : Element { }
    public class ElementType : Element { public string FamilyName { get; init; } = string.Empty; }
    public class Category { public string Name { get; init; } = string.Empty; }
    public class Family : Element { public Category? FamilyCategory { get; init; } }
    public class View : Element
    {
        public bool IsTemplate { get; init; }
        public ViewType ViewType { get; init; }
    }

    public class ViewSheet : View
    {
        public string SheetNumber { get; init; } = string.Empty;
    }

    public class Dimension : Element
    {
        public ElementId OwnerViewId { get; init; } = ElementId.InvalidElementId;
    }

    public class ViewSchedule : View
    {
        public static ViewSchedule CreateSchedule(Document document, ElementId categoryId) => new();
    }

    public sealed class Transaction : IDisposable
    {
        public Transaction(Document document, string name) { }
        public void Start() { }
        public void Commit() { }
        public void Dispose() { }
    }

    public class ElementSet { }

    public sealed class ElementId
    {
        public static readonly ElementId InvalidElementId = new(-1);
        public ElementId(long value) => Value = value;
        public long Value { get; }
    }

    public class Document
    {
        public string Title { get; init; } = "Untitled";
        public ProjectInfo? ProjectInformation { get; init; } = new();
        public Element? GetElement(ElementId id) => null;
    }

    public sealed class ProjectInfo
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed class FilteredElementCollector : List<Element>
    {
        public FilteredElementCollector(Document document) { }
        public FilteredElementCollector OfClass(Type type) => this;
        public FilteredElementCollector OfCategory(BuiltInCategory category) => this;
        public FilteredElementCollector WhereElementIsNotElementType() => this;
        public int GetElementCount() => Count;
        public ICollection<Element> ToElements() => this;
    }

    public enum BuiltInCategory
    {
        OST_Doors,
        OST_Windows,
        OST_Rooms,
        OST_Columns,
        OST_StructuralColumns
    }

    public enum ViewType
    {
        Undefined,
        FloorPlan,
        CeilingPlan,
        Section,
        Elevation,
        ThreeD,
        DrawingSheet
    }

    public enum BuiltInParameter
    {
        ALL_MODEL_MARK,
        CURVE_ELEM_LENGTH,
        WALL_USER_HEIGHT_PARAM,
        STRUCTURAL_MATERIAL_PARAM,
        FAMILY_LEVEL_PARAM,
        INSTANCE_REFERENCE_LEVEL_PARAM,
        ALL_MODEL_INSTANCE_COMMENTS
    }

    public enum StorageType
    {
        Double,
        ElementId,
        String
    }

    public sealed class Parameter
    {
        public StorageType StorageType { get; init; }
        public bool IsReadOnly { get; init; }
        public double AsDouble() => 0;
        public string? AsString() => null;
        public string? AsValueString() => null;
        public ElementId AsElementId() => ElementId.InvalidElementId;
        public void Set(string value) { }
    }

    public static class UnitUtils
    {
        public static double ConvertFromInternalUnits(double value, object unitTypeId) => value;
    }

    public static class UnitTypeId
    {
        public static object SquareMeters { get; } = new();
    }
}

namespace Autodesk.Revit.DB.Architecture
{
    public class Room : Autodesk.Revit.DB.Element
    {
        public new string Name { get; init; } = string.Empty;
        public double Area { get; init; }
    }
}

namespace Autodesk.Revit.UI
{
    using Autodesk.Revit.DB;

    public interface IExternalApplication
    {
        Result OnStartup(UIControlledApplication application);
        Result OnShutdown(UIControlledApplication application);
    }

    public interface IExternalCommand
    {
        Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements);
    }

    public enum Result
    {
        Succeeded,
        Failed,
        Cancelled
    }

    public class UIControlledApplication
    {
        public void CreateRibbonTab(string name) { }
        public RibbonPanel CreateRibbonPanel(string tabName, string panelName) => new();
    }

    public sealed class RibbonPanel
    {
        public void AddItem(PushButtonData data) { }
    }

    public sealed class PushButtonData
    {
        public PushButtonData(string name, string text, string assemblyName, string className) { }
        public string ToolTip { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
    }

    public sealed class ExternalCommandData
    {
        public UIApplication Application { get; init; } = new();
    }

    public sealed class UIApplication
    {
        public UIDocument? ActiveUIDocument { get; init; } = new();
    }

    public sealed class UIDocument
    {
        public Document Document { get; init; } = new();
    }

    public static class TaskDialog
    {
        public static void Show(string title, string message) { }
    }
}
#endif

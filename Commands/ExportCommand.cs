using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class ExportCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var service = new ExportService();
        string path = service.ExportBuilding(new RevitService(document).GetBuildingModel());
        TaskDialog.Show("Export Model", $"Model exported:{Environment.NewLine}{path}");
        return Result.Succeeded;
    }
}

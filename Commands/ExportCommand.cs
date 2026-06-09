using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class ExportCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var service = new ExportService();
        var revitService = new RevitService(document);
        var errors = new ValidationService(document, new RuleEngineService()).Validate();
        var results = service.ExportBuilding(revitService.GetBuildingModel(), errors);
        string paths = string.Join(Environment.NewLine, results.Select(result => $"{result.Format}: {result.Path}"));
        TaskDialog.Show("Export Model", $"Model exported:{Environment.NewLine}{paths}");
        return Result.Succeeded;
    }
}

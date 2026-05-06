using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class GenerateReportCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var revitService = new RevitService(document);
        var errors = new ValidationService(document, new RuleEngineService()).Validate();
        string path = new ReportService().Generate(revitService.GetBuildingModel(), errors);
        TaskDialog.Show("Generate Report", $"PDF report created:{Environment.NewLine}{path}");
        return Result.Succeeded;
    }
}

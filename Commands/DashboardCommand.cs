using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;
using BimAiAssistant.ViewModels;
using BimAiAssistant.Views;

namespace BimAiAssistant.Commands;

public sealed class DashboardCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var revitService = new RevitService(document);
        var model = revitService.GetBuildingModel();
        var errors = new ValidationService(document, new RuleEngineService()).Validate();
        var metrics = new DashboardService().BuildMetrics(model, errors);

        var window = new DashboardWindow
        {
            DataContext = new DashboardViewModel(model, metrics)
        };

        window.ShowDialog();
        return Result.Succeeded;
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class SyncCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var revitService = new RevitService(document);
        BuildingModel model = revitService.GetBuildingModel();
        var errors = new ValidationService(document, new RuleEngineService()).Validate();
        var history = new ChangeHistoryService().Load();
        var settingsService = new SettingsService();

        var payload = new SyncPayload
        {
            Organization = model.Organization,
            Project = model.Project,
            Model = model,
            ValidationIssues = errors,
            ChangeHistory = history
        };

        string response = new SyncService(settingsService, new AuthService(settingsService)).SyncAsync(payload).GetAwaiter().GetResult();
        TaskDialog.Show("Sync", $"Project synchronized with backend.{Environment.NewLine}{response}");
        return Result.Succeeded;
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;
using BimAiAssistant.ViewModels;
using BimAiAssistant.Views;

namespace BimAiAssistant.Commands;

public sealed class AICommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var settingsService = new SettingsService();
        var revitService = new RevitService(document);
        var viewModel = new AIViewModel(revitService, new OpenAIService(settingsService), settingsService);
        var window = new AIWindow { DataContext = viewModel };

        window.ShowDialog();
        return Result.Succeeded;
    }
}

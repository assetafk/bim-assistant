using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;
using BimAiAssistant.ViewModels;
using BimAiAssistant.Views;

namespace BimAiAssistant.Commands;

public sealed class SettingsCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var settingsService = new SettingsService();
        var window = new SettingsWindow { DataContext = new SettingsViewModel(settingsService) };
        window.ShowDialog();
        return Result.Succeeded;
    }
}
